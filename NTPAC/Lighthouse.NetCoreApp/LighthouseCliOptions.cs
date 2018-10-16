using System;
using CommandLine;

namespace Lighthouse.NetCoreApp
{
  public class LighthouseCliOptions
  {
    [Option('h', "hostname", Default = "127.0.0.1", HelpText = "IP address to listen on and to be *reachable* at")]
    public String ClusterNodeHostname { get; set; }

    [Option('p', "port", Default = 7070, HelpText = "Port")]
    public Int32 ClusterNodePort { get; set; }
  }
}
