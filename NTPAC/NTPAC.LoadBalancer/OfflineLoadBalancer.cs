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
  public class OfflineLoadBalancer : IPacketIngestor
  {
    private static readonly Config SystemConfig = ConfigurationFactory.ParseString(File.ReadAllText("loadBalancer.hocon"));
    private readonly IAkkaSettings _akkaSettings;

    private readonly ILogger _logger;
    private readonly IOfflineLoadBalancerActorFactory _offlineLoadBalancerActorFactory;

    public OfflineLoadBalancer(IAkkaSettings akkaSettings,
                               ILoggerFactory loggerFactory,
                               IOfflineLoadBalancerActorFactory offlineLoadBalancerActorFactory)
    {
      this._akkaSettings                    = akkaSettings;
      this._offlineLoadBalancerActorFactory = offlineLoadBalancerActorFactory;
      this._logger                          = loggerFactory.CreateLogger<OfflineLoadBalancer>();
    }

    private static Config Config =>
      ConfigurationFactory.ParseString(@"akka {
  stdout-loglevel = ERROR
  loglevel = ERROR
}");

    private static Config DebugConfig =>
      ConfigurationFactory.ParseString(@"akka {
  stdout-loglevel = DEBUG
  loglevel = DEBUG    
  actor {                
    debug {  
      receive = on 
      autoreceive = on
      lifecycle = on
      event-stream = on
      unhandled = on
    }
  }  
}");

    public IProcessingResult OpenCapture(Uri uri)
    {
      var openCaptureTask = Task.Run(async () => await this.OpenCaptureAsync(uri).ConfigureAwait(false));
      openCaptureTask.Wait();
      return openCaptureTask.Result;
    }

    public async Task<IProcessingResult> OpenCaptureAsync(Uri uri)
    {
      IProcessingResult processingResult;
      using (var system = ActorSystem.Create("NTPAC-Cluster", this._akkaSettings.IsDebug ? DebugConfig : Config))
      {
        var settings = new LoadBalancerSettings
                       {
                         BatchSize                      = 5000,
                         BatchRawCapturesSizeLimitBytes = 200_000,
                         BatchFlushInterval             = TimeSpan.FromSeconds(10),
                         ParallelBatchTransmissionsPerReassembler = 4
                       };

        var offlineLoadBalancerActor = this._offlineLoadBalancerActorFactory.Create(system, settings);
        var startProcessingRequest   = new CaptureProcessingRequest {CaptureInfo = new CaptureInfo(uri)};

        processingResult = await offlineLoadBalancerActor.Ask<ProcessingResult>(startProcessingRequest).ConfigureAwait(false);
        await CoordinatedShutdown.Get(system).Run(CoordinatedShutdown.ClusterLeavingReason.Instance).ConfigureAwait(false);
      }

      return processingResult;
    }
  }
}
