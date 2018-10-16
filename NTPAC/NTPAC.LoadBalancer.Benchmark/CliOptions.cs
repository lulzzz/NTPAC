using System;
using CommandLine;

namespace NTPAC.LoadBalancer.Benchmark
{
  internal class CliOptions
  {
    [Option('d',"directory", Required = false, HelpText = "Cassandra contact node's hostname")]
    public String BaseDirectory { get; set; }
    [Value(0, Required = false)]
    public String BaseDirectory1 { get; set; }
  }
}