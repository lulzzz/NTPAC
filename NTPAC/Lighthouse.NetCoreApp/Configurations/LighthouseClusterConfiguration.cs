using System;
using Akka.Configuration;

namespace Lighthouse.NetCoreApp.Configurations
{
  public class LighthouseClusterConfiguration : LighthouseConfiguration
  {
    public LighthouseClusterConfiguration(String hostName = null, Int32? specifiedPort = null) : base(hostName, specifiedPort) { }

    public override Config Config =>
      ConfigurationFactory.ParseString($@"akka.remote.dot-netty.tcp.hostname = ""0.0.0.0"" 
akka.remote.dot-netty.tcp.public-hostname = {this.HostName} 
akka.remote.dot-netty.tcp.port = {this.Port}");
  }
}
