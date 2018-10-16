using System;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using NTPAC.LoadBalancerCli;

namespace NTPAC.LoadBalancer.Benchmark
{
  [SimpleJob(RunStrategy.Throughput, 1, 1, 2, 1)]
  public class LoadBalancerBenchmark
  {
    public static String BaseDirectoryFullPath { get; } = System.Environment.GetEnvironmentVariable("baseDirectoryFullPath");

    [Benchmark]
    public void IsaHttp() { Run(GetPcapFilePath(@"isa-http.pcap")); }

    [Benchmark]
    public async Task IsaHttpAsync() { await RunAsync(GetPcapFilePath(@"isa-http.pcap")).ConfigureAwait(false); }

    [Benchmark]
    public void Sec6Net() { Run(GetPcapFilePath(@"sec6net.pcap")); }

    [Benchmark]
    public async Task Sec6NetAsync() { await RunAsync(GetPcapFilePath(@"sec6net.pcap")).ConfigureAwait(false); }

    [Benchmark]
    public void Sec6NetFiltered() { Run(GetPcapFilePath(@"sec6net-f.pcap")); }

    [Benchmark]
    public void Sec6NetFiltered1() { Run(GetPcapFilePath(@"sec6net-1gb.pcap")); }

    [Benchmark]
    public async Task Sec6NetFiltered1Async() { await RunAsync(GetPcapFilePath(@"sec6net-1gb.pcap")).ConfigureAwait(false); }

    [Benchmark]
    public async Task Sec6NetFilteredAsync() { await RunAsync(GetPcapFilePath(@"sec6net-f.pcap")).ConfigureAwait(false); }

    private static String GetPcapFilePath(String pcapFilePath) => Path.Join(BaseDirectoryFullPath, pcapFilePath);

    private static void Run(String pcapFilePath)
    {
      Console.Error.WriteLine(pcapFilePath);

      var lb = new LoadBalancerCliOptions {Uri = new Uri(pcapFilePath), DevnullRepository = true, Offline = true};
      LoadBalancerProgram.RunOptionsAndReturnExitCodeAsync(lb).Wait();
    }
    
    private static async Task RunAsync(String pcapFilePath)
    {
      Console.Error.WriteLine(pcapFilePath);

      var lb = new LoadBalancerCliOptions {Uri = new Uri(pcapFilePath), DevnullRepository = true, Offline = true};
      await LoadBalancerProgram.RunOptionsAndReturnExitCodeAsync(lb).ConfigureAwait(false);
    }
  }
}
