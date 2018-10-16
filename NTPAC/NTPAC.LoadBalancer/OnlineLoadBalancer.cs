using System;
using System.IO;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Microsoft.Extensions.Logging;
using NTPAC.Common.Interfaces;
using NTPAC.ConversatinTracking.Interfaces.Models;
using NTPAC.LoadBalancer.Interfaces;
using NTPAC.LoadBalancer.Messages;

namespace NTPAC.LoadBalancer
{
  public class OnlineLoadBalancer : IPacketIngestor, IClusterMember
  {
    private static readonly Config BaseConfig = ConfigurationFactory.ParseString(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "loadBalancer.hocon")));

    private readonly ILogger _logger;
    private readonly IOnlineLoadBalancerActorFactory _onlineLoadBalancerActorFactory;

    public OnlineLoadBalancer(IClusterSettings clusterSettings,
                              ILoggerFactory loggerFactory,
                              IOnlineLoadBalancerActorFactory onlineLoadBalancerActorFactory)
    {
      this._onlineLoadBalancerActorFactory = onlineLoadBalancerActorFactory;
      this.ClusterSettings                 = clusterSettings;
      this._logger                         = loggerFactory.CreateLogger<OnlineLoadBalancer>();
    }

    public IClusterSettings ClusterSettings { get; set; }

    public IProcessingResult OpenCapture(Uri uri)
    {
      var openCaptureTask = Task.Run(async () => await this.OpenCaptureAsync(uri).ConfigureAwait(false));
      openCaptureTask.Wait();
      return openCaptureTask.Result;
    }

    public async Task<IProcessingResult> OpenCaptureAsync(Uri uri)
    {
      IProcessingResult processingResult;
      
      var captureInfo = new CaptureInfo(uri);

      var settings = new LoadBalancerSettings(BaseConfig);

      var config = this.ClusterSettings.Config.WithFallback(BaseConfig);
      var seedNodes     = config.GetStringList("akka.cluster.seed-nodes");
      using (var system = ActorSystem.Create("NTPAC-Cluster", config))
      {
        var loadBalancerActor = this._onlineLoadBalancerActorFactory.Create(system, settings);
        var startProcessingRequest = new CaptureProcessingRequest {CaptureInfo = captureInfo};
        
        processingResult = await loadBalancerActor.Ask<ProcessingResult>(startProcessingRequest).ConfigureAwait(false);

        await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
        await CoordinatedShutdown.Get(system).Run(CoordinatedShutdown.ClusterLeavingReason.Instance).ConfigureAwait(false);
        
//        Console.WriteLine("Pre-dispose delay");
        await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
      }
      
//      Console.WriteLine("Post-dispose delay");
      await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

      return processingResult;
    }
  }
}
