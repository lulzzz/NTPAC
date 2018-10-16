using Akka.Actor;
using NTPAC.ConversatinTracking.Interfaces.Models;

namespace NTPAC.ConversatinTracking.Interfaces
{
  public interface ICaptureTrackingActorFactory
  {
    IActorRef Create(IActorContext context, CaptureInfo captureInfo, IActorRef contractor);
  }
}
