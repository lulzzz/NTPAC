using System;
using Akka.Configuration;

namespace NTPAC.Common.Interfaces
{
  public interface IClusterSettings
  {
    String ClusterNodeHostname { get; set; }
    Int32 ClusterNodePort { get; set; }
    String ClusterSeedNodeHostname { get; set; }
    Config Config { get; }
  }
}
