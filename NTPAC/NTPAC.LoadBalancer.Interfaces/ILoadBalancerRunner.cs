using System;
using System.Threading.Tasks;
using NTPAC.Common.Interfaces;

namespace NTPAC.LoadBalancer.Interfaces
{
  public interface ILoadBalancerRunner
  {
    Task<IProcessingResult> Run(Uri captureUri);
  }
}
