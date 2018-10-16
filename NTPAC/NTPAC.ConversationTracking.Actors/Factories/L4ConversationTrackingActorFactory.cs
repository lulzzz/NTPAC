using System;
using System.Net;
using Akka.Actor;
using NTPAC.ConversatinTracking.Interfaces;

namespace NTPAC.ConversationTracking.Actors.Factories
{
  public class L4ConversationTrackingActorFactory : IL4ConversationTrackingActorFactory
  {
    public IActorRef Create(IActorContext context,
                            IL4ConversationKey l4Key,
                            IActorRef contractor,
                            IPEndPoint sourceEndPoint,
                            IPEndPoint destinationEndPoint,
                            IActorRef l7ConversationStorageActor,
                            Int64 timestampTicks) =>
      context.ActorOf(L4ConversationTrackingActor.Props(l4Key, contractor, sourceEndPoint, destinationEndPoint,
                                                        l7ConversationStorageActor)
#if DEBUG
                      , $"{l4Key.ToString()}_{timestampTicks}"
#endif
        );
  }
}
