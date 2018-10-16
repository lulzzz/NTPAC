using System;
using System.IO;
using Akka.Configuration;

namespace Lighthouse.NetCoreApp.Configurations
{
  public abstract class LighthouseConfiguration
  {
    protected LighthouseConfiguration(String hostName = null, Int32? specifiedPort = null)
    {
      this.SetHostName(hostName);
      this.SetPort(specifiedPort);
    }

    public abstract Config Config { get; }

    public Config DefaultConfig => ConfigurationFactory.ParseString(File.ReadAllText("lightHouse.hocon"));
    public String HostName { get; private set; }
    public Int32 Port { get; private set; }
    public Config RemoteConfig => this.DefaultConfig.GetConfig("akka.remote");

    public String SystemName =>
      this.Config?.GetString("lighthouse.actorsystem") ?? this.DefaultConfig?.GetString("lighthouse.actorsystem") ?? "NotSet";

    private void SetHostName(String hostName)
    {
      this.HostName = hostName ?? this.RemoteConfig.GetString("dot-netty.tcp.public-hostname") ?? "127.0.0.1";
    }

    private void SetPort(Int32? specifiedPort)
    {
      this.Port = specifiedPort ?? this.RemoteConfig.GetInt("dot-netty.tcp.port");
      if (this.Port == 0)
      {
        throw new ConfigurationException(
          "Need to specify an explicit port for Lighthouse. Found an undefined port or a port value of 0 in App.config.");
      }
    }
  }
}
