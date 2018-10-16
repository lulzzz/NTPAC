using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using Akka.Event;
using Akka.Streams.Dsl;
using NTPAC.AkkaSupport;
using NTPAC.Common.Interfaces;
using NTPAC.LoadBalancer.Actors;
using NTPAC.LoadBalancer.Interfaces;
using NTPAC.LoadBalancer.Messages;
using NTPAC.Reassembling.IP;
using SharpPcap;

namespace NTPAC.Actors.BenchmarkImplementations
{
  public abstract class OnlineLoadBalancerBenchmarkActorBase : OnlineLoadBalancerActor
  {
    protected OnlineLoadBalancerBenchmarkActorBase(LoadBalancerSettings settings, IPcapLoader pcapLoader) : base(
      settings, pcapLoader)
    {
      this.MaxNumberOfShards = 1;
    }

    protected abstract Task MaterializePipeline(Source<RawCapture, NotUsed> packetSource, ActorSystem system);

    protected override void WaitingForCaptureBehaviour()
    {
      base.WaitingForCaptureBehaviour();
      this.Receive<BenchmarkRequest>(msg => this.OnBenchmarkRequest(msg));
    }

    private void OnBenchmarkRequest(BenchmarkRequest msg)
    {
      this.Log.Info("OnBenchmarkRequest");
      this.PcapLoader  = msg.PcapLoader;
      this.CaptureInfo = msg.CaptureInfo;
      this.IPIpv4DefragmentationEngine =
        new Ipv4DefragmentationEngine(new AkkaLoggingAdapter<Ipv4DefragmentationEngine>(Context.GetLogger()));
      this.DistributionSw = new Stopwatch();
      this.TotalSw        = new Stopwatch();
      this.StartTestBenchmarkAsync().PipeTo(this.Sender);
    }

    private async Task<ProcessingResult> StartTestBenchmarkAsync()
    {
      this.DistributionSw.Reset();
      this.TotalSw.Reset();

      var system       = Context.System;
      var packetSource = this.CreatePacketSource(this.CaptureInfo.Uri);

      this.TotalSw.Start();
      await this.MaterializePipeline(packetSource, system).ConfigureAwait(false);
      var totalTime = this.TotalSw.Elapsed;

      var processingResult = new ProcessingResult {Success = true, TotalTime = totalTime};
      return processingResult;
    }
  }
}
