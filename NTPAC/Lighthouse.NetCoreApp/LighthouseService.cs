using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Microsoft.Extensions.Hosting;

namespace Lighthouse.NetCoreApp
{
  public class LighthouseService : IHostedService, IDisposable
  {
    private readonly String _ipAddress;
    private readonly Int32? _port;

    private ActorSystem _lighthouseSystem;

    public LighthouseService(String ipAddress, Int32? port)
    {
      this._ipAddress = ipAddress;
      this._port      = port;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
      this._lighthouseSystem = LighthouseHostFactory.LaunchLighthouse(this._ipAddress, this._port);
      return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
      await this._lighthouseSystem.Terminate().ConfigureAwait(false);
    }

    public void Dispose()
    {
      this._lighthouseSystem?.Dispose();
    }
  }
}
