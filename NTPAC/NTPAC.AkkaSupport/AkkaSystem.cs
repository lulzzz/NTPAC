using System;
using Akka.Actor;

namespace NTPAC.AkkaSupport
{
  public class AkkaSystem
  {
    public AkkaSystem()
    {
      // create a new actor system (a container for actors)
      var system = ActorSystem.Create("NTPAC");

      // create actor and get a reference to it.
      // this will be an "ActorRef", which is not a 
      // reference to the actual actor instance
      // but rather a client or proxy to it
      //var captureActor = system.ActorOf(Capture.Props(new CaptureInfo("test")));

      // send a message to the actor
      // captureActor.Tell(new ProcessPacketRequest(0,null));

      // prevent the application from exiting before message is handled
      Console.ReadLine();
    }
  }
}
