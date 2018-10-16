using System;
using System.Text;
using System.Text.RegularExpressions;
using Akka.Configuration;
using Akka.Event;
using NTPAC.Common.Interfaces;

namespace NTPAC.Common
{
  public class ClusterSettings : IClusterSettings
  {
    public String ClusterNodeHostname { get; set; }
    public Int32 ClusterNodePort { get; set; }
    public String ClusterSeedNodeHostname { get; set; }

    public Config Config
    {
      get
      {
        var sb = new StringBuilder();
        if (!String.IsNullOrEmpty(this.ClusterNodeHostname))
        {
          sb.AppendLine($"akka.remote.dot-netty.tcp.public-hostname = \"{this.ClusterNodeHostname}\"");
          sb.AppendLine("akka.remote.dot-netty.tcp.hostname = \"0.0.0.0\"");
        }

        if (this.ClusterNodePort != 0)
        {
          sb.AppendLine($"akka.remote.dot-netty.tcp.port = \"{this.ClusterNodePort}\"");
        }

        if (!String.IsNullOrEmpty(this.ClusterSeedNodeHostname))
        {
          // Add default port if ommited 
          var portRegexMatch = new Regex(@":\d+$").Match(this.ClusterSeedNodeHostname);
          if (!portRegexMatch.Success)
          {
            this.ClusterSeedNodeHostname += ":7070";
          } 
          sb.AppendLine($"akka.cluster.seed-nodes = [\"akka.tcp://NTPAC-Cluster@{this.ClusterSeedNodeHostname}\"]");
        }

        sb.AppendLine();

        return ConfigurationFactory.ParseString(sb.ToString());
      }
    }
  }
}
