using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Sharding;
using Akka.Event;
using Akka.Persistence;
using NTPAC.AkkaSupport;
using NTPAC.Common.Interfaces;
using NTPAC.ConversatinTracking.Interfaces.Models;
using NTPAC.ConversationTracking.Actors;
using NTPAC.ConversationTracking.Helpers;
using NTPAC.LoadBalancer.Actors.Online;
using NTPAC.LoadBalancer.Interfaces;
using NTPAC.LoadBalancer.Messages;
using NTPAC.Messages;
using NTPAC.Messages.CaptureTracking;
using NTPAC.Messages.RawPacket;
using NTPAC.Messages.Sharding;
using PacketDotNet;
using SharpPcap;

namespace NTPAC.LoadBalancer.Actors
{
  public class SimpleOnlineLoadBalancerActor : ReceiveActor
  {
    private readonly ILoggingAdapter _logger = Context.GetLogger();
    private readonly Cluster _cluster = Cluster.Get(Context.System);
    private readonly IActorRef _clusterProxy = ClusterSharding.Get(Context.System).StartProxy(CaptureControllerActor.TypeName,
                                                                                               CaptureControllerActor.ReassemblerClusterRoleName,
                                                                                               new ReassemblerEntityMessageExtractor());
    
    private readonly LoadBalancerSettings _settings;
    private IPcapLoader _pcapLoader;
   
    private IActorRef _contractor;
    private CaptureInfo _captureInfo;

    private AskableReassemblerEntityMessageProxy askProxy; 
    
    public SimpleOnlineLoadBalancerActor(LoadBalancerSettings settings, IPcapLoader pcapLoader)
    {
      this._pcapLoader = pcapLoader;
      this._settings   = settings;
      
      this.Become(this.WaitingForCaptureProcessingRequestsBehaviour);
    }
    
    public static Props Props(LoadBalancerSettings settings, IPcapLoader pcapLoader) =>
      Akka.Actor.Props.Create(() => new SimpleOnlineLoadBalancerActor(settings, pcapLoader));
    
    private void WaitingForCaptureProcessingRequestsBehaviour()
    {
      this.Receive<CaptureProcessingRequest>(msg => this.OnCaptureProcessingRequest(msg)); 
    }

    private void ProcessingCaptureBehaviour()
    {
      this.Receive<CaptureTrackingCompleted>(completed => this.OnCaptureTrackingCompleted(completed));
      this.Receive<CaptureInfoRequest>(request => this.OnCaptureInfoRequest());
      this.Receive<RawPacketBatchAck>(batchAck => this.OnRawPacketBatchAck(batchAck));  
    }

    private void OnCaptureInfoRequest()
    {
      this.Sender.Tell(this._captureInfo);
    }

    private void OnCaptureTrackingCompleted(CaptureTrackingCompleted completed)
    {
    }
    
    private void OnCaptureProcessingRequest(CaptureProcessingRequest request)
    {
      this._captureInfo = request.CaptureInfo;
      this._contractor = this.Sender;
      
      this.askProxy = new AskableReassemblerEntityMessageProxy(this.Self, this._clusterProxy);

      this.Become(this.ProcessingCaptureBehaviour);
      this.TryStartProcessingAsync().PipeTo(this.Self);
      
      
      this.Become(this.ProcessingCaptureBehaviour);
    }

    private void OnRawPacketBatchAck(RawPacketBatchAck ack)
    {
      this.askProxy.MatchReceivedMessage(ack);
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
      try
      {
        this._pcapLoader.Open(this._captureInfo.Uri);

        RawCapture rawCapture;
        while ((rawCapture = this._pcapLoader.GetNextPacket()) != null)
        {
          var parsedPacket = this.ParsePacket(rawCapture);
//          if (!this.TryGetL3L4ConversationKey(parsedPacket, out var l3L4ConversationKey))
//          {
//            continue;
//          }
        }
      }
      finally
      {
        this._pcapLoader.Close();
        this._pcapLoader.Dispose();
      }
      
      return null;
    }

    private Packet ParsePacket(RawCapture rawCapture)
    {
      Packet parsedPacket;
      try
      {
        parsedPacket = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);
      }
      catch (ArgumentOutOfRangeException ex)
      {
        throw new Exception($"Parsing error {ex}");
      }
      return parsedPacket;
    }

//    private Boolean TryGetL3L4ConversationKey(Packet parsedPacket, out L3L4ConversationKey l3L4ConversationKey)
//    {
//      // Ignore non-IP traffic (STP, ... )
//      if (!(parsedPacket?.PayloadPacket is IPPacket ipPacket))
//      {
//        return false;
//      }
//
//      // Attempt to defragment
//      if (ipPacket is IPv4Packet ipv4Packet && Ipv4Helpers.Ipv4PacketIsFragmented(ipv4Packet))
//      {
//        return false;
//      }
//
//      // Ignore unsupported transport protocols
//      var transportPacket = ipPacket.PayloadPacket;
//      if (!(transportPacket is TcpPacket || transportPacket is UdpPacket))
//      {
//        return false;
//      }
//
//      l3L4ConversationKey = new L3L4ConversationKey(ipPacket);
//      return true;
//    }
  }
}
