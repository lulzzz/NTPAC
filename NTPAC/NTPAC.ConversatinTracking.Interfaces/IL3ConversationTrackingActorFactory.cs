using Akka.Actor;

namespace NTPAC.ConversatinTracking.Interfaces
{
  public interface IL3ConversationTrackingActorFactory
  {
    IActorRef Create(IActorContext context, IL3ConversationKey l3Key, IActorRef contractor, IActorRef l7ConversationStorageActor);
  }
}
