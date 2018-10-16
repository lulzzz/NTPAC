using System;
using BenchmarkDotNet.Running;

namespace Mappings.Benchmark
{
  //  $ dotnet.exe .\bin\Release\netcoreapp2.1\Mappings.Benchmark.dll
  internal class Program
  {
    private static void Main(String[] args)
    {
#if DEBUG
      var pcapLoaderBenchmark = new MappingBenchmark();
      pcapLoaderBenchmark.ManualMapperBenchmark();
      pcapLoaderBenchmark.AutoMapperBenchmark();
#else
      BenchmarkRunner.Run<MappingBenchmark>();
#endif
    }
  }
}
