using System;
using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using NTPAC.ConversatinTracking.Interfaces;

namespace NTPAC.ConversationTracking.Actors.Factories
{
  public class L3ConversationTrackingActorFactory : IL3ConversationTrackingActorFactory
  {
    private readonly IServiceProvider _serviceProvider;
    public L3ConversationTrackingActorFactory(IServiceProvider serviceProvider) => this._serviceProvider = serviceProvider;

    public IActorRef Create(IActorContext context,
                            IL3ConversationKey l3Key,
                            IActorRef contractor,
                            IActorRef l7ConversationStorageActor) =>
      context.ActorOf(L3ConversationTrackingActor.Props(l3Key, contractor, l7ConversationStorageActor,
                                                        this._serviceProvider
                                                            .GetRequiredService<IL4ConversationTrackingActorFactory>())
#if DEBUG
                     , l3Key.ToString()
#endif
        );
  }
}
