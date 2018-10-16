using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Sharding;
using Akka.Event;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.Streams.Util;
using NTPAC.AkkaSupport;
using NTPAC.AkkaSupport.GraphStages;
using NTPAC.Common.Interfaces;
using NTPAC.ConversatinTracking.Interfaces.Models;
using NTPAC.ConversationTracking.Actors;
using NTPAC.ConversationTracking.Factories;
using NTPAC.ConversationTracking.Helpers;
using NTPAC.LoadBalancer.Actors.Online;
using NTPAC.LoadBalancer.Interfaces;
using NTPAC.LoadBalancer.Messages;
using NTPAC.Messages;
using NTPAC.Messages.CaptureTracking;
using NTPAC.Messages.RawPacket;
using NTPAC.Messages.Sharding;
using NTPAC.Reassembling.IP;
using PacketDotNet;
using SharpPcap;

namespace NTPAC.LoadBalancer.Actors
{
  public class OnlineLoadBalancerActor : ReceiveActor
  {
    protected readonly ILoggingAdapter Log = Context.GetLogger();
    protected readonly Cluster Cluster = Cluster.Get(Context.System);
    protected readonly IActorRef ClusterProxy = ClusterSharding.Get(Context.System).StartProxy(CaptureControllerActor.TypeName,
                                                                                               CaptureControllerActor.ReassemblerClusterRoleName,
                                                                                               new ReassemblerEntityMessageExtractor());
    
    protected readonly LoadBalancerSettings Settings;
    protected CaptureInfo CaptureInfo;
    
    protected IActorRef Contractor;
    protected Stopwatch DistributionSw;
    protected Ipv4DefragmentationEngine IPIpv4DefragmentationEngine;
    protected IPcapLoader PcapLoader;
    protected IActorRef SelfLoadBalancerActor;
    protected Stopwatch TotalSw;
    protected AskableReassemblerEntityMessageProxy ReassemblerMessageProxy;

    protected Int32 MaxNumberOfShards;
    
    protected readonly TaskCompletionSource<Boolean> WaitForLoadbalancerUpStateTCS = new TaskCompletionSource<Boolean>();

    public OnlineLoadBalancerActor(LoadBalancerSettings settings, IPcapLoader pcapLoader)
    {
      this.PcapLoader = pcapLoader;
      this.Settings   = settings;
      this.Become(this.WaitingForCaptureBehaviour);
    }

    public static Props Props(LoadBalancerSettings settings, IPcapLoader pcapLoader) =>
      Akka.Actor.Props.Create(() => new OnlineLoadBalancerActor(settings, pcapLoader));

    protected override void PreStart()
    {
      this.Cluster.Subscribe(this.Self, ClusterEvent.InitialStateAsEvents, new []{ typeof(ClusterEvent.IMemberEvent), typeof(ClusterEvent.UnreachableMember) });
    }
    
    protected Source<RawCapture, NotUsed> CreatePacketSource(Uri uri) =>
      Source.UnfoldResource(
        // Capture opening
        () =>
        {
          this.PcapLoader.Open(uri);
          return this.PcapLoader;
        },
        // Capture reading
        pcapLoader =>
        {
          var rawCapture = pcapLoader.GetNextPacket();
          return rawCapture != null ? new Option<RawCapture>(rawCapture) : Option<RawCapture>.None;
        },
        // Capture closing
        pcapLoader => pcapLoader.Close());

    protected IMaybeMultipleValues<RawPacket> ParsePacket(RawCapture rawCapture)
    {
      if (rawCapture == null)
      {
        return new ProcessRawPacketRequestError("Missing packet payload");
      }

      
//      return new RawPacket(rawCapture) {EntityId = 0};
      

      
      Packet parsedPacket;
      try
      {
        parsedPacket = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);
      }
      catch (ArgumentOutOfRangeException ex)
      {
        return new ProcessRawPacketRequestError($"Parsing error {ex}");
      }

      // Ignore non-IP traffic (STP, ... )
      if (!(parsedPacket?.PayloadPacket is IPPacket ipPacket))
      {
        return new ProcessRawPacketRequestError("Non-IP packet");
      }

      // Attempt to defragment
      if (ipPacket is IPv4Packet ipv4Packet && Ipv4Helpers.Ipv4PacketIsFragmented(ipv4Packet))
      {
        var (isDefragmentationSuccessful, firstTimestamp, defragmentedIpv4Packet) =
          this.IPIpv4DefragmentationEngine.TryDefragmentFragments(
            FrameFactory.CreateFromIpPacket(ipv4Packet, rawCapture.Timeval.Date.Ticks));
        if (!isDefragmentationSuccessful)
        {
          return ProcessRawPacketFragmentsRequest.EmptyInstance;
        }

        var defragmentedL3L4Key = new L3L4ConversationKey(defragmentedIpv4Packet);
        return new RawPacket(firstTimestamp, LinkLayers.Raw, defragmentedIpv4Packet.Bytes, defragmentedL3L4Key,
                             this.MaxNumberOfShards);
      }

      // Ignore unsupported transport protocols
      var transportPacket = ipPacket.PayloadPacket;
      if (!(transportPacket is TcpPacket || transportPacket is UdpPacket))
      {
        return new ProcessRawPacketRequestError("Unsupported transport protocol");
      }

      var l3L4Key = new L3L4ConversationKey(ipPacket);
      return new RawPacket(rawCapture, l3L4Key, this.MaxNumberOfShards);
    }

    private void SetupGarbageCollecting()
    {
      this.Receive<CaptureProcessingRequest>(msg => this.OnStartProcessingRequest(msg));
      this.Receive<CollectGarbageRequest>(msg => this.OnCollectGarbageRequest());

      Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2), this.Self,
                                                      CollectGarbageRequest.Instance, this.Self);
    }
    
    protected virtual void WaitingForCaptureBehaviour()
    {     
      this.Receive<CaptureProcessingRequest>(msg => this.OnStartProcessingRequest(msg));
      
      this.SetupGarbageCollecting();
    }

    private Source<UInt64, NotUsed> CreatePipeline(Source<RawCapture, NotUsed> packetSource, Int32 reassemblerNodes) =>
      packetSource 
        // Parse packets and construct RawPacket objects with EntityId values set
        // EntityId is calculated from hash of the L3L4ConversationKey
        .Select(this.ParsePacket)
        // Insert async boundary to enable execution in two separate threads
        .Async()
        // Extract embedded values if any
        .MaybeSelectMultipleValues()
        // Filter out invalid packets (future feature: channel invalid packets into the logging sink)
        .WhereNot(processRawPacketRequest => processRawPacketRequest is ProcessRawPacketRequestError)
        // Split stream of packets based on their EntityId into the corresponding substreams
        .GroupBy(this.MaxNumberOfShards, processRawPacketRequest => processRawPacketRequest.EntityId)
        // Batch packets (by max size and interval) in the substreams
        .GroupedByCountAndRawCaptureSizeWithin(this.Settings.BatchSize, this.Settings.BatchRawCapturesSizeLimitBytes,
                                               this.Settings.BatchFlushInterval)
        // Append sequence counter to each batch (for each shard individually)
        .ZipWithIndex()
        // Send each batch to given node
        .SelectAsyncUnordered(this.Settings.ParallelBatchTransmissionsPerReassembler * reassemblerNodes,
                              packetBatchWithSeqId =>
                                this.SendPacketBatchAsync(packetBatchWithSeqId.Item1, packetBatchWithSeqId.Item2))
        // Workaround to solve the substream's RunWith NotImplementedException
        .MergeSubstreams() as Source<UInt64, NotUsed>;

    private void OnCaptureInfoRequest() { this.Sender.Tell(this.CaptureInfo); }

    private void OnCollectGarbageRequest()
    {
      this.Log.Info("Collecting garbage");
      GC.Collect();
      this.Log.Info("Garbage collected");
    }
    
    private void OnRawPacketBatchAck(RawPacketBatchAck ack)
    {
      this.ReassemblerMessageProxy.MatchReceivedMessage(ack);
    }

    private void OnStartProcessingRequest(CaptureProcessingRequest request)
    {
      this.Contractor            = this.Sender;
      this.SelfLoadBalancerActor = this.Self;
      this.CaptureInfo           = request.CaptureInfo;
      
      this.DistributionSw = new Stopwatch();
      this.TotalSw        = new Stopwatch();
      this.IPIpv4DefragmentationEngine =
        new Ipv4DefragmentationEngine(new AkkaLoggingAdapter<Ipv4DefragmentationEngine>(Context.GetLogger()));
      this.ReassemblerMessageProxy = new AskableReassemblerEntityMessageProxy(this.SelfLoadBalancerActor, this.ClusterProxy);

      this.Become(this.ProcessingCaptureBehaviour);
      this.TryStartProcessingAsync().PipeTo(this.Self);
    }

    private void ProcessingCaptureBehaviour()
    {
      this.Receive<CaptureTrackingCompleted>(completed => this.OnCaptureTrackingCompleted(completed));
      this.Receive<CaptureInfoRequest>(request => this.OnCaptureInfoRequest());
      this.Receive<RawPacketBatchAck>(batchAck => this.OnRawPacketBatchAck(batchAck));
      this.Receive<ProcessingResult>(processingResult => this.OnProcessingResult(processingResult));
      
      this.Receive<ClusterEvent.IMemberEvent>(memberEvent => this.OnIMemberEvent(memberEvent));
      
      this.SetupGarbageCollecting();
    }

    private void OnIMemberEvent(ClusterEvent.IMemberEvent memberEvent)
    {
      if (memberEvent.Member.UniqueAddress.Equals(this.Cluster.SelfUniqueAddress) && memberEvent.Member.Status == MemberStatus.Up)
      {
        this.WaitForLoadbalancerUpStateTCS.SetResult(true);
      }
    }
    
    private void OnProcessingResult(ProcessingResult processingResult)
    {
      this.Contractor.Tell(processingResult);
      Context.Stop(this.Self);
    }

    private void OnCaptureTrackingCompleted(CaptureTrackingCompleted completed)
    {
      this.ReassemblerMessageProxy.MatchReceivedMessage(completed);
    }

    private async Task SendCaptureTrackingCompleteToReassemblersAsync()
    {
      var shardingStats = await this.ClusterProxy.Ask<ClusterShardingStats>(new GetClusterShardingStats(TimeSpan.FromSeconds(60)))
                            .ConfigureAwait(false);
      var entityIds = shardingStats.Regions.Values.SelectMany(regionStats => regionStats.Stats.Keys).Select(Int32.Parse);

      var receiveTasks = entityIds
                         .Select(entityId =>
                                   this.ReassemblerMessageProxy.Ask<CaptureTrackingCompleted>(
                                     entityId, new CaptureTrackingComplete())).Cast<Task>();

      var receiveTaskAllCompletedTask = Task.WhenAll(receiveTasks);

      await Task.WhenAny(receiveTaskAllCompletedTask, Task.Delay(this.Settings.AskableMessageReplyTimeout)).ConfigureAwait(false);
      if (!receiveTaskAllCompletedTask.IsCompleted)
      {
        throw new TimeoutException($"Failed to receive all requested CaptureTrackingCompleted messages on time ({this.Settings.AskableMessageReplyTimeout.Seconds} s)"); 
      }
    }

    private async Task<UInt64> SendPacketBatchAsync(IEnumerable<RawPacket> processRawPacketRequests, Int64 batchSeqId)
    {    
      if (!this.DistributionSw.IsRunning)
      {
        this.DistributionSw.Start();
      }

      var processRawPacketRequestsList = processRawPacketRequests as List<RawPacket> ?? processRawPacketRequests.ToList();
      if (!processRawPacketRequestsList.Any())
      {
        return 0;
      }

      var entityId = processRawPacketRequestsList[0].EntityId;
      var batch    = new RawPacketBatchRequest(processRawPacketRequestsList, batchSeqId);

//      return (UInt64) processRawPacketRequestsList.Count;
      
//      var envelope = new ReassemblerEntityMessageEnvelope(entityId, batch);
//      this.ClusterProxy.Tell(envelope, this.SelfLoadBalancerActor);
  
      var receiveTask =  (Task) this.ReassemblerMessageProxy.Ask<RawPacketBatchAck>(entityId, batch);
      await Task.WhenAny(receiveTask, Task.Delay(this.Settings.AskableMessageReplyTimeout)).ConfigureAwait(false);
      if (!receiveTask.IsCompleted)
      {
        throw new TimeoutException($"Failed to receive RawPacketBatchAck on time ({this.Settings.AskableMessageReplyTimeout.Seconds} s)");
      }
      
      var sentRawPacketRequests = (UInt64) processRawPacketRequestsList.Count;
      return sentRawPacketRequests;
    }

    private async Task<ProcessingResult> TryStartProcessingAsync()
    {
      try
      {
        return await this.StartProcessingAsync().ConfigureAwait(false);
      }
      catch (Exception e)
      {
        return new ProcessingResult { Success = false, Exception = e };
      }
    }
    
    private async Task<ProcessingResult> StartProcessingAsync()
    {    
      this.DistributionSw.Reset();
      this.TotalSw.Reset();

      await this.WaitForLoadbalancerUpStateTCS.Task.ConfigureAwait(false);

      var reassemblerNodes = this.Cluster.State.Members
                                   .Where(member => member.HasRole(CaptureControllerActor.ReassemblerClusterRoleName) && member.Status == MemberStatus.Up).ToList();
      if (!reassemblerNodes.Any())
      {
        this.Log.Error("No Up reassemblers");
        return new ProcessingResult
               {
                 Success          = false,
               };
      }

      this.MaxNumberOfShards = reassemblerNodes.Count * this.Settings.ShardsPerEntity;
      
      this.Log.Info($"{reassemblerNodes.Count} Up reassemblers: [{String.Join(", ", reassemblerNodes.Select(member => member.Address.ToString()))}]");
      
      var packetSource = this.CreatePacketSource(this.CaptureInfo.Uri);
      var pipeline     = this.CreatePipeline(packetSource, reassemblerNodes.Count);
      if (pipeline == null)
      {
        throw new Exception("Failed to initialize the pipeline");
      }

      var materializerSettings = ActorMaterializerSettings.Create(Context.System).WithInputBuffer(1024, 1024);

      UInt64 processedPackets;
      using (var materializer = Context.System.Materializer(materializerSettings))
      {
        this.TotalSw.Start();
        processedPackets = await pipeline.RunWith(Sink.Aggregate<UInt64, UInt64>(0, (sum, n) => sum + n), materializer)
                                           .ConfigureAwait(false);
      }
      
      var distributionTime = this.DistributionSw.Elapsed;

      this.Log.Info("Finalizing ...");
      await this.SendCaptureTrackingCompleteToReassemblersAsync().ConfigureAwait(false);
      var totalTime = this.TotalSw.Elapsed;

      this.Log.Info("Leaving cluster ...");
      await this.Cluster.LeaveAsync().ConfigureAwait(false);
      
      return new ProcessingResult
             {
               Success          = true,
               ProcessedPackets = processedPackets,
               DistributionTime = distributionTime,
               TotalTime        = totalTime,
               CaptureSize      = this.PcapLoader.CaptureSize
             };            
    }
  }
}
