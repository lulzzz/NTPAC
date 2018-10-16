using System;
using CommandLine;

namespace NTPAC.PcapLoader.Benchmark
{
  public class CliOptions
  {
    [Option('d', "directory", Required = false, HelpText = "Test files' base directory")]
    public String BaseDirectory { get; set; }
  }
}
