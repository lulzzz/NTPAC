using System;
using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using NTPAC.ConversatinTracking.Interfaces;
using NTPAC.ConversatinTracking.Interfaces.Models;
using NTPAC.Persistence.Interfaces;

namespace NTPAC.ConversationTracking.Actors.Factories
{
  public class L7ConversationStorageActorFactory : IL7ConversationStorageActorFactory
  {
    private readonly IServiceProvider _serviceProvider;

    public L7ConversationStorageActorFactory(IServiceProvider serviceProvider) => this._serviceProvider = serviceProvider;

    public IActorRef Create(IActorContext context, CaptureInfo captureInfo, IActorRef contractor) =>
      context.ActorOf(L7ConversationStorageActor.Props(captureInfo, contractor,
                                                       this._serviceProvider.GetRequiredService<ICaptureFacade>(),
                                                       this._serviceProvider.GetRequiredService<IL7ConversationFacade>()),
                      "L7CS");
  }
}
