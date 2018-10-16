using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using NTPAC.Common;
using NTPAC.Common.Interfaces;
using NTPAC.ConversatinTracking.Interfaces;
using NTPAC.ConversationTracking.Actors.Factories;
using NTPAC.LoadBalancer;
using NTPAC.LoadBalancer.Actors;
using NTPAC.LoadBalancer.Actors.Factories;
using NTPAC.LoadBalancer.Actors.Offline;
using NTPAC.LoadBalancer.Interfaces;
using NTPAC.PcapLoader;
using NTPAC.Persistence.Cassandra.Facades;
using NTPAC.Persistence.Cassandra.Facades.Installers;
using NTPAC.Persistence.DevNull.Facades;
using NTPAC.Persistence.InMemory.Facades;
using NTPAC.Persistence.Interfaces;

[assembly: InternalsVisibleTo("NTPAC.LoadBalancer.Benchmark")]

namespace NTPAC.LoadBalancerCli
{
  public class LoadBalancerProgram
  {
    private static ILoadBalancerRunnerFactory _loadBalancerRunnerFactory;

    internal static async Task<Int32> RunOptionsAndReturnExitCodeAsync(LoadBalancerCliOptions opts)
    {
      if (opts.Uri.IsFile && !File.Exists(opts.Uri.AbsolutePath))
      {
        Console.Error.WriteLine($"{nameof(LoadBalancerProgram)} {opts.Uri}: No such file");
        return 1;
      }
      
      ConfigureServices(opts);
      
      var sw = new Stopwatch();
      sw.Start();

      var runner = _loadBalancerRunnerFactory.CreateInstance();

      var processingResult = await runner.Run(opts.Uri).ConfigureAwait(false);

      Console.WriteLine(processingResult.ToString());
      
      Console.WriteLine(
        $"{nameof(LoadBalancerProgram)} has taken {sw.Elapsed} to process {opts.Uri.AbsolutePath} in {(opts.Offline ? "offline" : "online")} mode");

      return 0;
    }

    private static Boolean CheckCmdOptions(LoadBalancerCliOptions cliOptions)
    {
      if (!cliOptions.Offline &&
          (cliOptions.CassandraRepository || cliOptions.DevnullRepository || cliOptions.InMemoryRepository))
      {
        Console.WriteLine("Repository cannot be defined in online mode!");
        return false;
      }

      return true;
    }

    private static void ConfigureServices(LoadBalancerCliOptions opts)
    {
      IServiceCollection serviceCollection = new ServiceCollection();
      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(serviceCollection);
      serviceCollection.AddSingleton(provider => provider);
      serviceCollection.AddSingleton<IPcapLoader, PcapLoader.PcapLoader>();

      serviceCollection.AddSingleton<LoadBalancerRunner>();
      serviceCollection.AddSingleton<IAkkaSettings>(new AkkaSettings {IsDebug = opts.IsDebug});

      if (opts.Uri.Scheme == "rpcap")
      {
        serviceCollection.AddSingleton<ICaptureDeviceFactory, CaptureLiveDeviceFactory>();
      }
      else
      {
        serviceCollection.AddSingleton<ICaptureDeviceFactory, CaptureDeviceFactory>();
      }

      serviceCollection.AddSingleton<ICaptureFacade, CaptureFacade>();
      serviceCollection.AddSingleton<IL7ConversationFacade, L7ConversationFacade>();

      serviceCollection.AddSingleton<ILoadBalancerRunner, LoadBalancerRunner>();
      serviceCollection.AddSingleton<ILoadBalancerRunnerFactory, LoadBalancerRunnerFactory>();
      serviceCollection.AddSingleton<IBatchLoader, BatchLoader>();
      serviceCollection.AddSingleton<IBatchSender, BatchSender>();
      serviceCollection.AddSingleton<IRawPacketBatchParserActorFactory, RawPacketBatchParserActorFactory>();
      serviceCollection.AddSingleton<ICaptureTrackingActorFactory, CaptureTrackingActorFactory>();
      serviceCollection.AddSingleton<IL3ConversationTrackingActorFactory, L3ConversationTrackingActorFactory>();
      serviceCollection.AddSingleton<IL4ConversationTrackingActorFactory, L4ConversationTrackingActorFactory>();
      serviceCollection.AddSingleton<IL7ConversationStorageActorFactory, L7ConversationStorageActorFactory>();

      if (opts.Offline)
      {
        serviceCollection.AddSingleton<IPacketIngestor, OfflineLoadBalancer>();
        serviceCollection.AddSingleton<IOfflineLoadBalancerActorFactory, OfflineLoadBalancerActorFactory>();
      }
      else
      {
        serviceCollection.AddSingleton<IPacketIngestor, OnlineLoadBalancer>();
        serviceCollection.AddSingleton<IClusterSettings, ClusterSettings>(
          provider => new ClusterSettings
                      {
                        ClusterNodeHostname     = opts.ClusterNodeHostname,
                        ClusterNodePort         = opts.ClusterNodePort,
                        ClusterSeedNodeHostname = opts.ClusterSeedNodeHostname
                      });
        serviceCollection.AddSingleton<IClusterMember, OnlineLoadBalancer>();
        serviceCollection.AddSingleton<IOnlineLoadBalancerActorFactory, OnlineLoadBalancerActorFactory>();
      }

      if (opts.DevnullRepository)
      {
        DevNullServiceInstaller.Install(serviceCollection);
      }
      else if (opts.CassandraRepository)
      {
        CassandraServiceInstaller.Install(serviceCollection, opts.CassandraKeyspace, opts.CassandraContactPoint);
      }
      // Not specified or InMemoryRepository selected
      else
      {
        InMemoryServiceInstaller.Install(serviceCollection);
      }

      var services = serviceCollection.BuildServiceProvider();

      services.GetService<ILoggerFactory>().AddProvider(opts.IsDebug ?
                                                          new ConsoleLoggerProvider((s, level) => level >= LogLevel.Debug, true) :
                                                          new ConsoleLoggerProvider((s, level) => level >= LogLevel.Error, true));

      _loadBalancerRunnerFactory = services.GetService<ILoadBalancerRunnerFactory>();
    }

    private static async Task<Int32> Main(String[] args)
    {
      LoadBalancerCliOptions cliOptions = null;
      Parser.Default.ParseArguments<LoadBalancerCliOptions>(args).WithParsed(opt => cliOptions = opt);

      if (cliOptions == null || !CheckCmdOptions(cliOptions))
      {
        return 1;
      }

      return await RunOptionsAndReturnExitCodeAsync(cliOptions).ConfigureAwait(false);
    }
  }
}
