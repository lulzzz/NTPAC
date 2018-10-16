using System;
using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using NTPAC.Common.Interfaces;
using NTPAC.LoadBalancer.Interfaces;

namespace NTPAC.LoadBalancer.Actors.Factories
{
  public class OnlineLoadBalancerActorFactory : IOnlineLoadBalancerActorFactory
  {
    private readonly IServiceProvider _serviceProvider;

    public OnlineLoadBalancerActorFactory(IServiceProvider serviceProvider) => this._serviceProvider = serviceProvider;

    public IActorRef Create(ActorSystem actorSystem, LoadBalancerSettings settings) =>
      actorSystem.ActorOf(OnlineLoadBalancerActor.Props(settings, this._serviceProvider.GetRequiredService<IPcapLoader>()));
  }
}
