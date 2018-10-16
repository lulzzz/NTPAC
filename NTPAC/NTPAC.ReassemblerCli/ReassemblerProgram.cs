using System;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NTPAC.Common;
using NTPAC.Common.Interfaces;
using NTPAC.ConversatinTracking.Interfaces;
using NTPAC.ConversationTracking.Actors.Factories;
using NTPAC.ConversationTracking.Actors.Mailboxes;
using NTPAC.Persistence.Cassandra.Facades;
using NTPAC.Persistence.Cassandra.Facades.Installers;
using NTPAC.Persistence.DevNull.Facades;
using NTPAC.Persistence.InMemory.Facades;
using NTPAC.Persistence.Interfaces;

namespace NTPAC.ReassemblerCli
{
  internal class ReassemblerProgram
  {
    private static void ConfigureServices(IServiceCollection services, ReassemblerCliOptions opts)
    {
      services.AddLogging();

      services.AddSingleton<IClusterSettings, ClusterSettings>(
        provider => new ClusterSettings
                    {
                      ClusterNodeHostname     = opts.ClusterNodeHostname,
                      ClusterNodePort         = opts.ClusterNodePort,
                      ClusterSeedNodeHostname = opts.ClusterSeedNodeHostname
                    });

      services.AddSingleton<IHostedService, ReassemblerService>();

      services.AddSingleton<ICaptureFacade, CaptureFacade>();
      services.AddSingleton<IL7ConversationFacade, L7ConversationFacade>();
      services.AddSingleton<ICaptureTrackingActorFactory, CaptureTrackingActorFactory>();
      services.AddSingleton<IRawPacketBatchParserActorFactory, RawPacketBatchParserActorFactory>();
      services.AddSingleton<ICaptureTrackingActorFactory, CaptureTrackingActorFactory>();
      services.AddSingleton<IL3ConversationTrackingActorFactory, L3ConversationTrackingActorFactory>();
      services.AddSingleton<IL4ConversationTrackingActorFactory, L4ConversationTrackingActorFactory>();
      services.AddSingleton<IL7ConversationStorageActorFactory, L7ConversationStorageActorFactory>();

      if (opts.DevnullRepository)
      {
        DevNullServiceInstaller.Install(services);
      }
       else if (opts.CassandraRepository)
      {
        CassandraServiceInstaller.Install(services, opts.CassandraKeyspace, opts.CassandraContactPoint);
      }
      else
      {
        InMemoryServiceInstaller.Install(services);
      }
    }

    private static async Task<Int32> Main(String[] args)
    {
      ReassemblerCliOptions cliOptions = null;
      Parser.Default.ParseArguments<ReassemblerCliOptions>(args).WithParsed(options => cliOptions = options);
      if (cliOptions == null)
      {
        return 1;
      }
      
      var hostBuilder = new HostBuilder().ConfigureServices((hostContext, services) =>
      {
       ConfigureServices(services, cliOptions);
      });
      await hostBuilder.RunConsoleAsync().ConfigureAwait(false);

      return 0;
    }
  }
}
