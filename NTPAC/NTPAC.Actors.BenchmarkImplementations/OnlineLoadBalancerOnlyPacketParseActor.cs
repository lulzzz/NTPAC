using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using NTPAC.Common.Interfaces;
using NTPAC.LoadBalancer.Interfaces;
using NTPAC.Messages.RawPacket;
using SharpPcap;

namespace NTPAC.Actors.BenchmarkImplementations
{
  public class OnlineLoadBalancerOnlyPacketParseActor : OnlineLoadBalancerBenchmarkActorBase
  {
    public OnlineLoadBalancerOnlyPacketParseActor(LoadBalancerSettings settings, IPcapLoader pcapLoader) : base(
      settings, pcapLoader)
    {
    }

    public new static Props Props(LoadBalancerSettings settings, IPcapLoader pcapLoader) =>
      Akka.Actor.Props.Create(() => new OnlineLoadBalancerOnlyPacketParseActor(settings, pcapLoader));

    protected override async Task MaterializePipeline(Source<RawCapture, NotUsed> packetSource, ActorSystem system)
    {
      var pipeline = packetSource.Select(this.ParsePacket);

      using (var materializer = system.Materializer())
      {
        await pipeline.RunWith(Sink.Ignore<IMaybeMultipleValues<RawPacket>>(), materializer).ConfigureAwait(false);
      }
    }
  }
}
