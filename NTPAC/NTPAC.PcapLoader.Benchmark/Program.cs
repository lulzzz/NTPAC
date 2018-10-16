using System;
using System.IO;
using BenchmarkDotNet.Running;
using CommandLine;

namespace NTPAC.PcapLoader.Benchmark
{
  //  $ dotnet.exe .\bin\Release\netcoreapp2.1\NTPAC.PcapLoader.Benchmark.dll
  internal class Program
  {
    private static void Main(String[] args)
    {
      CliOptions opts = null;
      Parser.Default.ParseArguments<CliOptions>(args).WithParsed(options => { opts = options; });

      var baseDirectoryFullPath = opts?.BaseDirectory ??
                                  Environment.GetEnvironmentVariable(PcapLoaderBenchmark.BaseDirectoryFullPathEnvName) ??
                                  Path.GetFullPath($"{Directory.GetCurrentDirectory()}/../TestingData");
      Environment.SetEnvironmentVariable(PcapLoaderBenchmark.BaseDirectoryFullPathEnvName, baseDirectoryFullPath);
      
      BenchmarkRunner.Run<PcapLoaderBenchmark>();
    }
  }
}
