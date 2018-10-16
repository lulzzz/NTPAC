using Akka.Actor;
using NTPAC.ConversatinTracking.Interfaces.Models;

namespace NTPAC.ConversatinTracking.Interfaces
{
  public interface IRawPacketBatchParserActorFactory
  {
    IActorRef Create(IActorContext context, IActorRef contractor, CaptureInfo captureInfo);
  }
}
