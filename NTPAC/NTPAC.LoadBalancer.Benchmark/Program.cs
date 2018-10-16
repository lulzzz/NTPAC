using System;
using System.IO;
using BenchmarkDotNet.Running;
using CommandLine;

namespace NTPAC.LoadBalancer.Benchmark
{
  //  $ dotnet.exe .\bin\Release\netcoreapp2.1\NTPAC.LoadBalancer.Benchmark.dll
  internal class Program
  {
    private static void Main(String[] args)
    {
      Parser.Default.ParseArguments<CliOptions>(args)
            .WithParsed(options =>
            {
              var baseDirectoryFullPath = options.BaseDirectory ?? options.BaseDirectory1 ?? @"c:\";

              System.Environment.SetEnvironmentVariable("baseDirectoryFullPath", baseDirectoryFullPath);
            });

      

      BenchmarkRunner.Run<LoadBalancerBenchmark>();

      //var loadBalancerBenchmark = new LoadBalancerBenchmark();
      //loadBalancerBenchmark.IsaHttp();
    }
  }
}
