using System;
using System.Net;
using Akka.Actor;

namespace NTPAC.ConversatinTracking.Interfaces
{
  public interface IL4ConversationTrackingActorFactory
  {
    IActorRef Create(IActorContext context,
                     IL4ConversationKey l4Key,
                     IActorRef contractor,
                     IPEndPoint sourceEndPoint,
                     IPEndPoint destinationEndPoint,
                     IActorRef l7ConversationStorageActor,
                     Int64 timestampTicks
                     );
  }
}
