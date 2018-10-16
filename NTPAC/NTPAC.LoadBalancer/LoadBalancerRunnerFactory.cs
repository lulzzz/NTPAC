using System;
using Microsoft.Extensions.DependencyInjection;
using NTPAC.LoadBalancer.Interfaces;

namespace NTPAC.LoadBalancer
{
  public class LoadBalancerRunnerFactory : ILoadBalancerRunnerFactory
  {
    private readonly IServiceProvider _serviceProvider;

    public LoadBalancerRunnerFactory(IServiceProvider serviceProvider) => this._serviceProvider = serviceProvider;

    public ILoadBalancerRunner CreateInstance() => this._serviceProvider.GetService<ILoadBalancerRunner>();
  }
}
