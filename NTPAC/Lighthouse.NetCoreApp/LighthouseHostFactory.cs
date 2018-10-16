using System;
using Akka.Actor;
using Lighthouse.NetCoreApp.Configurations;

namespace Lighthouse.NetCoreApp
{
  /// <summary>
  ///   Launcher for the Lighthouse <see cref="ActorSystem" />
  /// </summary>
  public static class LighthouseHostFactory
  {
    public static ActorSystem LaunchLighthouse(String hostname = null, Int32? specifiedPort = null)
    {
      var lighthouseLocal                         = new LighthouseClusterConfiguration(hostname, specifiedPort);
      var lighthouseClusterConfiguration          = new LighthouseDefaultConfiguration(hostname, specifiedPort);
      var lighthouseClusterConfigurationWithSeeds = new LighthouseClusterWithSeedsConfiguration(hostname, specifiedPort);

      var finalConfig = lighthouseLocal.Config.WithFallback(lighthouseClusterConfigurationWithSeeds.Config)
                                       .WithFallback(lighthouseClusterConfiguration.Config);
      var systemName = finalConfig.GetString("lighthouse.actorsystem") ?? "NotSet";

      return ActorSystem.Create(systemName, finalConfig);
    }
  }
}
