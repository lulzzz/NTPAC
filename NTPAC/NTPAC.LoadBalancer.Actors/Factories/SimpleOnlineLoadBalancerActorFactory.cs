using System;
using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using NTPAC.Common.Interfaces;
using NTPAC.LoadBalancer.Interfaces;

namespace NTPAC.LoadBalancer.Actors.Factories
{
  public class SimpleOnlineLoadBalancerActorFactory : IOnlineLoadBalancerActorFactory
  {
    private readonly IServiceProvider _serviceProvider;

    public SimpleOnlineLoadBalancerActorFactory(IServiceProvider serviceProvider) => this._serviceProvider = serviceProvider;
    
    public IActorRef Create(ActorSystem actorSystem, LoadBalancerSettings settings) => 
      actorSystem.ActorOf(SimpleOnlineLoadBalancerActor.Props(settings, this._serviceProvider.GetRequiredService<IPcapLoader>()));
  }
}
