using System;
using System.Linq;
using Akka.Configuration;

namespace Lighthouse.NetCoreApp.Configurations
{
  public class LighthouseClusterWithSeedsConfiguration : LighthouseConfiguration
  {
    public LighthouseClusterWithSeedsConfiguration(String hostName = null, Int32? specifiedPort = null) :
      base(hostName, specifiedPort) =>
      this.Config = this.GetInjectedClusterConfigString();

    public override Config Config { get; }

    private String GetInjectedClusterConfigString()
    {
      var selfAddress = $"akka.tcp://{this.SystemName}@{this.HostName}:{this.Port}";
      var seeds       = this.DefaultConfig.GetStringList("akka.cluster.seed-nodes");
      if (!seeds.Contains(selfAddress))
      {
        seeds.Add(selfAddress);
      }

      var injectedClusterConfigString =
        seeds.Aggregate("akka.cluster.seed-nodes = [", (current, seed) => current + @"""" + seed + @""", ");
      injectedClusterConfigString += "]";
      return injectedClusterConfigString;
    }
  }
}
