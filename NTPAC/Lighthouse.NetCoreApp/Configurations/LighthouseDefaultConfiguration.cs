using System;
using Akka.Configuration;

namespace Lighthouse.NetCoreApp.Configurations
{
  public class LighthouseDefaultConfiguration : LighthouseConfiguration
  {
    public LighthouseDefaultConfiguration(String hostName = null, Int32? specifiedPort = null) : base(hostName, specifiedPort) { }

    public override Config Config => this.DefaultConfig;
  }
}
