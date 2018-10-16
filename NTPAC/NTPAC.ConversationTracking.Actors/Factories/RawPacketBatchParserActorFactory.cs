using System;
using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using NTPAC.ConversatinTracking.Interfaces;
using NTPAC.ConversatinTracking.Interfaces.Models;

namespace NTPAC.ConversationTracking.Actors.Factories
{
  public class RawPacketBatchParserActorFactory : IRawPacketBatchParserActorFactory
  {
    private readonly IServiceProvider _serviceProvider;

    public RawPacketBatchParserActorFactory(IServiceProvider serviceProvider) => this._serviceProvider = serviceProvider;

    public IActorRef Create(IActorContext context, IActorRef contractor, CaptureInfo captureInfo) =>
      context.ActorOf(RawPacketBatchParserActor.Props(this._serviceProvider.GetRequiredService<ICaptureTrackingActorFactory>(),
                                                      contractor, captureInfo));
  }
}
