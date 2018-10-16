using System;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.XunitLogger;
using NTPAC.Actors.BenchmarkImplementations;
using NTPAC.Common.Interfaces;
using NTPAC.ConversatinTracking.Interfaces.Models;
using NTPAC.LoadBalancer.Actors;
using NTPAC.LoadBalancer.Interfaces;
using NTPAC.LoadBalancer.Messages;
using NTPAC.PcapLoader;
using Xunit;
using Xunit.Abstractions;

namespace NTPAC.Actors.Benchmarks
{
  public class LoadBalancerActorTests : TestKit
  {
    private readonly ITestOutputHelper _output;
    private readonly IServiceProvider _services;

    private readonly LoadBalancerSettings _settings = new LoadBalancerSettings
                                                      {
                                                        BatchSize          = 4000,
                                                        BatchFlushInterval = TimeSpan.FromSeconds(10),
                                                        ParallelBatchTransmissionsPerReassembler = 
                                                          Environment.ProcessorCount
                                                      };

    public LoadBalancerActorTests(ITestOutputHelper output)
    {
      this._output = output;

      IServiceCollection serviceCollection = new ServiceCollection();
      ConfigureServices(serviceCollection);
      this._services = serviceCollection.BuildServiceProvider();
      this._services.GetService<ILoggerFactory>().AddProvider(new XunitLoggerProvider(LogLevel.Debug, output));
    }

    [Theory(Skip = "Benchmark")]
    [InlineData(@"c:\small.pcap")]
    [InlineData(@"c:\big.pcap")]
    [InlineData(@"c:\sec6net.pcap")]
    public void ParsingBenchmark_Complete(String pcapFilePath)
    {
      this.RunBenchmark(pcapFilePath,
                        this.Sys.ActorOf(OnlineLoadBalancerCompleteActor.Props(
                                           this._settings, this._services.GetRequiredService<IPcapLoader>())));
    }

    [Theory(Skip = "Benchmark")]
    [InlineData(@"c:\small.pcap")]
    [InlineData(@"c:\big.pcap")]
    [InlineData(@"c:\sec6net.pcap")]
    public void ParsingBenchmark_CompleteTunned(String pcapFilePath)
    {
      this.RunBenchmark(pcapFilePath,
                        this.Sys.ActorOf(OnlineLoadBalancerCompleteTunnedActor.Props(
                                           this._settings, this._services.GetRequiredService<IPcapLoader>())));
    }

    [Theory(Skip = "Benchmark")]
    [InlineData(@"c:\small.pcap")]
    [InlineData(@"c:\big.pcap")]
    [InlineData(@"c:\sec6net.pcap")]
    public void ParsingBenchmark_GroupedTunned(String pcapFilePath)
    {
      this.RunBenchmark(pcapFilePath,
                        this.Sys.ActorOf(OnlineLoadBalancerGroupedTunnedActor.Props(
                                           this._settings, this._services.GetRequiredService<IPcapLoader>())));
    }

    [Theory(Skip = "Benchmark")]
    [InlineData(@"c:\small.pcap")]
    [InlineData(@"c:\big.pcap")]
    [InlineData(@"c:\sec6net.pcap")]
    public void ParsingBenchmark_GroupedWithin(String pcapFilePath)
    {
      this.RunBenchmark(pcapFilePath,
                        this.Sys.ActorOf(OnlineLoadBalancerGroupedWithinActor.Props(
                                           this._settings, this._services.GetRequiredService<IPcapLoader>())));
    }

    [Theory(Skip = "Benchmark")]
    [InlineData(@"c:\small.pcap")]
    [InlineData(@"c:\big.pcap")]
    [InlineData(@"c:\sec6net.pcap")]
    public void ParsingBenchmark_GroupedWithin_Tunned(String pcapFilePath)
    {
      this.RunBenchmark(pcapFilePath,
                        this.Sys.ActorOf(OnlineLoadBalancerGroupedWithinTunnedActor.Props(
                                           this._settings, this._services.GetRequiredService<IPcapLoader>())));
    }

    [Theory(Skip = "Benchmark")]
    [InlineData(@"c:\small.pcap")]
    [InlineData(@"c:\big.pcap")]
    [InlineData(@"c:\sec6net.pcap")]
    public void ParsingBenchmark_OnlyParsePackets(String pcapFilePath)
    {
      this.RunBenchmark(pcapFilePath,
                        this.Sys.ActorOf(OnlineLoadBalancerOnlyPacketParseActor.Props(
                                           this._settings, this._services.GetRequiredService<IPcapLoader>())));
    }

    [Theory(Skip = "Benchmark")]
    [InlineData(@"c:\small.pcap")]
    [InlineData(@"c:\big.pcap")]
    [InlineData(@"c:\sec6net.pcap")]
    [InlineData(@"r:\sec6net.pcap")]
    public void ParsingBenchmark_UserTell(String pcapFilePath)
    {
      this.RunBenchmark(pcapFilePath,
                        this.Sys.ActorOf(OnlineLoadBalancerActor.Props(this._settings,
                                                                       this._services.GetRequiredService<IPcapLoader>())));
    }

    private static void ConfigureServices(IServiceCollection serviceCollection)
    {
      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(serviceCollection);
      serviceCollection.AddSingleton<IPcapLoader, PcapLoader.PcapLoader>();
      serviceCollection.AddSingleton<ICaptureDeviceFactory, CaptureDeviceFactory>();
      serviceCollection.AddSingleton<IPacketIngestor, PacketIngestorLog>();
    }

    private void RunBenchmark(String pcapFilePath, IActorRef subject)
    {
      var captureInfo = new CaptureInfo(new Uri(pcapFilePath));
      var req         = new BenchmarkRequest {CaptureInfo = captureInfo, PcapLoader = this._services.GetService<IPcapLoader>()};

      var probe = this.CreateTestProbe();

      subject.Tell(req, probe);
      var result = probe.ExpectMsg<ProcessingResult>(TimeSpan.FromSeconds(60));
      this._output.WriteLine($"Total time: {result.TotalTime}");
    }
  }
}
