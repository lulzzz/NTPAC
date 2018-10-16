using System;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using NTPAC.AkkaSupport.GraphStages;
using NTPAC.Common.Interfaces;
using NTPAC.LoadBalancer.Interfaces;
using NTPAC.Messages.RawPacket;
using SharpPcap;

namespace NTPAC.Actors.BenchmarkImplementations
{
  public class OnlineLoadBalancerCompleteActor : OnlineLoadBalancerBenchmarkActorBase
  {
    public OnlineLoadBalancerCompleteActor(LoadBalancerSettings settings, IPcapLoader pcapLoader) : base(settings, pcapLoader) { }

    public new static Props Props(LoadBalancerSettings settings, IPcapLoader pcapLoader) =>
      Akka.Actor.Props.Create(() => new OnlineLoadBalancerCompleteActor(settings, pcapLoader));

    protected override async Task MaterializePipeline(Source<RawCapture, NotUsed> packetSource, ActorSystem system)
    {
      var pipeline = this.CreatePipeline(packetSource);

      using (var materializer = system.Materializer())
      {
        await pipeline.RunWith(Sink.Ignore<Int32>(), materializer).ConfigureAwait(false);
      }
    }

    private Source<Int32, NotUsed> CreatePipeline(Source<RawCapture, NotUsed> packetSource) =>
      packetSource
        // Parse packets and construct RawPacket objects with EntityId values set
        // EntityId is calculated from hash of the L3L4ConversationKey
        //.SelectAsync(Environment.ProcessorCount, this.ParsePacketAsync)
        .Select(this.ParsePacket)
        // Extract embedded values if any
        .Via(new MaybeSelectMultipleValues<RawPacket>())
        // Filter out invalid packets (future feature: channel invalid packets into the logging sink)
        .WhereNot(processRawPacketRequest => processRawPacketRequest is ProcessRawPacketRequestError)
        // Split stream of packets based on their EntityId into the corresponding substreams
        .GroupBy(this.MaxNumberOfShards, processRawPacketRequest => processRawPacketRequest.EntityId)
        // Batch packets (by max size and interval) in the substreams
        .GroupedWithin(this.Settings.BatchSize, this.Settings.BatchFlushInterval)
        // Append sequence counter to each batch (for each shard individually)
        .ZipWithIndex()
        // Send each batch to given node
        .SelectAsyncUnordered(this.Settings.ParallelBatchTransmissionsPerReassembler, packetBatchWithSeqId => Task.FromResult(0))
        // Workaround to solve the substream's RunWith NotImplementedException
        .MergeSubstreams() as Source<Int32, NotUsed>;
  }
}
