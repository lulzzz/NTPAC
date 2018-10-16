using System;
using System.Threading.Tasks;
using NTPAC.Common.Interfaces;
using NTPAC.LoadBalancer.Interfaces;

namespace NTPAC.LoadBalancer
{
  public class LoadBalancerRunner : ILoadBalancerRunner
  {
    private readonly IPacketIngestor _packetIngestor;

    public LoadBalancerRunner(IPacketIngestor packetIngestor) => this._packetIngestor = packetIngestor;

    public async Task<IProcessingResult> Run(Uri captureUri) => await this._packetIngestor.OpenCaptureAsync(captureUri).ConfigureAwait(false);
  }
}
