using Akka.Actor;
using NTPAC.ConversatinTracking.Interfaces.Models;

namespace NTPAC.ConversatinTracking.Interfaces
{
  public interface IL7ConversationStorageActorFactory
  {
    IActorRef Create(IActorContext context, CaptureInfo captureInfo, IActorRef contractor);
  }
}
