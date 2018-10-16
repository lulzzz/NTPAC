using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Configuration;
using Microsoft.Extensions.Hosting;
using NTPAC.Common.Interfaces;
using NTPAC.ConversatinTracking.Interfaces;
using NTPAC.ConversationTracking.Actors;
using NTPAC.Messages.Sharding;

namespace NTPAC.ReassemblerCli
{
  public class ReassemblerService : IHostedService, IDisposable
  {
    private static readonly Config SystemConfig = ConfigurationFactory.ParseString(File.ReadAllText("reassembler.hocon"));
    
    private readonly IRawPacketBatchParserActorFactory _rawPacketBatchParserActorFactory;
    private readonly IClusterSettings _clusterSettings;
    private ActorSystem _system;
    
    public ReassemblerService(IClusterSettings clusterSettings, IRawPacketBatchParserActorFactory rawPacketBatchParserActorFactory)
    {
      this._rawPacketBatchParserActorFactory = rawPacketBatchParserActorFactory;
      this._clusterSettings = clusterSettings;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
      var config = this._clusterSettings.Config.WithFallback(SystemConfig);
      this._system = ActorSystem.Create("NTPAC-Cluster", config);
      
      ClusterSharding.Get(this._system).Start(CaptureControllerActor.TypeName,
                                              CaptureControllerActor.Props(this._rawPacketBatchParserActorFactory),
                                              ClusterShardingSettings.Create(this._system).WithRole(CaptureControllerActor.ReassemblerClusterRoleName),
                                              new ReassemblerEntityMessageExtractor());

      this._system.Scheduler.Advanced.ScheduleRepeatedly(TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2), () =>
      {
        this._system.Log.Info("Collecting garbage");
        GC.Collect();
        this._system.Log.Info("Garbage collected");
      });
    }

    public async Task StopAsync(CancellationToken cancellationToken) {
      await CoordinatedShutdown.Get(this._system).Run(CoordinatedShutdown.ClusterLeavingReason.Instance).ConfigureAwait(false); 
    }
    
    public void Dispose()
    {
      this._system?.Dispose();
    }
  }
}
