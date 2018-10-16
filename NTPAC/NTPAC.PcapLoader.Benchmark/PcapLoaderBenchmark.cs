using System;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Microsoft.Extensions.DependencyInjection;
using NTPAC.Common.Extensions;
using NTPAC.Common.Interfaces;

namespace NTPAC.PcapLoader.Benchmark
{
  [SimpleJob(RunStrategy.Throughput, 1, 1, 2, 1)]
  public class PcapLoaderBenchmark
  {
    public static readonly String BaseDirectoryFullPathEnvName = "BASE_DIRECTORY_FULL_PATH";
    public static String BaseDirectoryFullPath { get; } = Environment.GetEnvironmentVariable(BaseDirectoryFullPathEnvName);

    private ServiceProvider _services;

//    [Benchmark]
//    public void Big() { this.Run(TestingPcap); }
//
//    [Benchmark]
//    public async Task BigAsync() { await this.RunAsync(TestingPcap).ConfigureAwait(false); }
//
//    [Benchmark]
//    public void IsaHttp() { this.Run(TestingPcap); }

//    [Benchmark]
//    public async Task IsaHttpAsync() { await this.RunAsync(TestingPcap).ConfigureAwait(false); }

    [Benchmark]
    public void Sec6NetF() { this.Run("sec6net-f.pcap"); }

    [Benchmark]
    public async Task Sec6NetFAsync() { await this.RunAsync("sec6net-f.pcap").ConfigureAwait(false); }
    
    [Benchmark]
    public void Sec6NetGb() { this.Run("sec6net-1gb.pcap"); }

    [Benchmark]
    public async Task Sec6NetGbAsync() { await this.RunAsync("sec6net-1gb.pcap").ConfigureAwait(false); }

    [Benchmark]
    public void Small() { this.Run("various_bigger.pcapng"); }

    [Benchmark]
    public async Task SmallAsync() { await this.RunAsync("various_bigger.pcapng").ConfigureAwait(false); }

    [GlobalSetup]
    public void Setup()
    {
      IServiceCollection serviceCollection = new ServiceCollection();
      ConfigureServices(serviceCollection);
      this._services = serviceCollection.BuildServiceProvider();
      //this._services.GetService<ILoggerFactory>().AddProvider(new XunitLoggerProvider(LogLevel.Debug, output));
    }

    private static void ConfigureServices(IServiceCollection serviceCollection)
    {
      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(serviceCollection);
      serviceCollection.AddTransient<IPcapLoader, PcapLoader>();
      serviceCollection.AddSingleton<ICaptureDeviceFactory, CaptureDeviceFactory>();
      serviceCollection.AddTransient<IPacketIngestor, PacketIngestorLog>();
    }

    private void Run(String pcapFileName)
    {
      var fileInfo     = new FileInfo(Path.Combine(BaseDirectoryFullPath, pcapFileName));
      var pcapIngestor = this._services.GetService<IPacketIngestor>();

      pcapIngestor.OpenCapture(fileInfo.ToUri());
    }

    private async Task RunAsync(String pcapFileName)
    {
      var fileInfo     = new FileInfo(Path.Combine(BaseDirectoryFullPath, pcapFileName));
      var pcapIngestor = this._services.GetService<IPacketIngestor>();

      await Task.Run(async () => await pcapIngestor.OpenCaptureAsync(fileInfo.ToUri()).ConfigureAwait(false))
                .ConfigureAwait(false);
    }
  }
}
