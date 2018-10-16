using System.Collections.Generic;
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
  public class OnlineLoadBalancerGroupedWithinTunnedActor : OnlineLoadBalancerBenchmarkActorBase
  {
    public OnlineLoadBalancerGroupedWithinTunnedActor(LoadBalancerSettings settings, IPcapLoader pcapLoader) : base(
      settings, pcapLoader)
    {
    }

    public new static Props Props(LoadBalancerSettings settings, IPcapLoader pcapLoader) =>
      Akka.Actor.Props.Create(() => new OnlineLoadBalancerGroupedWithinTunnedActor(settings, pcapLoader));

    protected override async Task MaterializePipeline(Source<RawCapture, NotUsed> packetSource, ActorSystem system)
    {
      var pipeline = this.CreatePipeline(packetSource);

      var materializerSettings = ActorMaterializerSettings.Create(system).WithInputBuffer(512, 512);

      using (var materializer = system.Materializer(materializerSettings))
      {
        await pipeline.RunWith(Sink.Ignore<IEnumerable<RawPacket>>(), materializer).ConfigureAwait(false);
      }
    }

    private Source<IEnumerable<RawPacket>, NotUsed> CreatePipeline(Source<RawCapture, NotUsed> packetSource) =>
      packetSource
        // Parse packets and construct RawPacket objects with EntityId values set
        // EntityId is calculated from hash of the L3L4ConversationKey
        //.SelectAsync(Environment.ProcessorCount, this.ParsePacketAsync)
        .Select(this.ParsePacket)
        //
        .Async()
        // Extract embedded values if any
        .Via(new MaybeSelectMultipleValues<RawPacket>())
        // Filter out invalid packets (future feature: channel invalid packets into the logging sink)
        .WhereNot(processRawPacketRequest => processRawPacketRequest is ProcessRawPacketRequestError)
        //
        //.Async()
        // Split stream of packets based on their EntityId into the corresponding substreams
        .GroupBy(this.MaxNumberOfShards, processRawPacketRequest => processRawPacketRequest.EntityId)
        // Batch packets (by max size and interval) in the substreams
        .GroupedWithin(this.Settings.BatchSize, this.Settings.BatchFlushInterval)
        .MergeSubstreams() as Source<IEnumerable<RawPacket>, NotUsed>;
  }
}
