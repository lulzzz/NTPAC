using System;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lighthouse.NetCoreApp
{
  internal class LighthouseProgram
  {
    private static async Task Main(String[] args)
    {
      LighthouseCliOptions cliOptions = null;
      Parser.Default.ParseArguments<LighthouseCliOptions>(args).WithParsed(options => cliOptions = options);
      if (cliOptions == null)
      {
        return;
      }
      await RunLighthouse(cliOptions).ConfigureAwait(false);
    }

    private static async Task RunLighthouse(LighthouseCliOptions opts)
    {
      var hostBuilder = new HostBuilder().ConfigureServices((hostContext, services) =>
                                                            {
                                                              services.AddSingleton<IHostedService, LighthouseService>(
                                                                provider => new LighthouseService(opts.ClusterNodeHostname, opts.ClusterNodePort));
                                                            });
      await hostBuilder.RunConsoleAsync().ConfigureAwait(false);
    }
  }
}
