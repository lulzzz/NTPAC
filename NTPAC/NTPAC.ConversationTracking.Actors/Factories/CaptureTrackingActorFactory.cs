using System;
using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using NTPAC.ConversatinTracking.Interfaces;
using NTPAC.ConversatinTracking.Interfaces.Models;

namespace NTPAC.ConversationTracking.Actors.Factories
{
  public class CaptureTrackingActorFactory : ICaptureTrackingActorFactory
  {
    private readonly IServiceProvider _serviceProvider;

    public CaptureTrackingActorFactory(IServiceProvider serviceProvider) => this._serviceProvider = serviceProvider;

    public IActorRef Create(IActorContext context, CaptureInfo captureInfo, IActorRef contractor) =>
      context.ActorOf(CaptureTrackingActor.Props(captureInfo, contractor,
                                                 this._serviceProvider.GetRequiredService<IL3ConversationTrackingActorFactory>(),
                                                 this._serviceProvider.GetRequiredService<IL7ConversationStorageActorFactory>()));
  }
}
