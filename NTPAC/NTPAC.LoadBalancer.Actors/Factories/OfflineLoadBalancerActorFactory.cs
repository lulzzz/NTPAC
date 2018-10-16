using System;
using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using NTPAC.ConversatinTracking.Interfaces;
using NTPAC.LoadBalancer.Interfaces;

namespace NTPAC.LoadBalancer.Actors.Factories
{
  public class OfflineLoadBalancerActorFactory : IOfflineLoadBalancerActorFactory
  {
    private readonly IServiceProvider _serviceProvider;

    public OfflineLoadBalancerActorFactory(IServiceProvider serviceProvider) => this._serviceProvider = serviceProvider;

    public IActorRef Create(ActorSystem actorSystem, LoadBalancerSettings settings) =>
      actorSystem.ActorOf(OfflineLoadBalancerActor.Props(this._serviceProvider.GetRequiredService<IBatchLoader>(),
                                                         this._serviceProvider.GetRequiredService<IBatchSender>(), settings,
                                                         this._serviceProvider
                                                             .GetRequiredService<IRawPacketBatchParserActorFactory>()));
  }
}
