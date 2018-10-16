using System;
using System.IO;
using Akka.Actor;
using Akka.Configuration;
using SharpPcap;
using SharpPcap.LibPcap;

namespace NTPAC.Akka.TestCli
{
  internal class Program
  {
    private static void Die(String msg, Int32 exitCode = 1)
    {
      Console.Error.WriteLine(msg);
      Environment.Exit(exitCode);
    }

    private static void Main(String[] args)
    {
      //IServiceCollection serviceCollection = new ServiceCollection();
      //ConfigureServices(serviceCollection);
      //this._services = serviceCollection.BuildServiceProvider();
      ////this._services.GetService<ILoggerFactory>().AddProvider(new XunitLoggerProvider(LogLevel.Debug,output));

      //// create a new actor system (a container for actors)
      //var system = ActorSystem.Create("MySystem");

      //// create actor and get a reference to it.
      //// this will be an "ActorRef", which is not a 
      //// reference to the actual actor instance
      //// but rather a client or proxy to it
      //var greeter = system.ActorOf(Capture.Props(new CaptureInfo("test")));

      //var 
      //// send a message to the actor
      //greeter.Tell(new Greet("World"));

      //// prevent the application from exiting before message is handled
      //Console.ReadLine();

      var pcapPath = "/Users/vilco/Desktop/isa-http.pcap";
      //String pcapPath = @"c:\big.pcap";
      if (!File.Exists(pcapPath))
      {
        Die($"{pcapPath}: No such file or directory");
      }


      var systemConfig = ConfigurationFactory.ParseString("akka {loglevel = \"OFF\"  }");

      using (var system = ActorSystem.Create("NTPAC"))
      {
//                Console.WriteLine("Creating capture actor");
//                var captureActor = system.ActorOf(Capture.Props(new CaptureInfo(pcapPath)), "testCapture");
//
//                ProcessCapture(pcapPath, captureActor);

        Console.ReadLine();
      }

      Console.WriteLine("Done");
    }

    private static void ProcessCapture(String pcapPath, IActorRef captureActor)
    {
      ICaptureDevice captureDevice = new CaptureFileReaderDevice(pcapPath);
      try
      {
        Console.WriteLine("Opening capture device");
        captureDevice.Open();

        var rawCapture = captureDevice.GetNextPacket();
        while (rawCapture != null)
        {
          //Console.WriteLine("Incoming packet");
          var taskRawCapture = rawCapture;
          //Task.Run(() => {
          //    var parsedPacket = Packet.ParsePacket(taskRawCapture.LinkLayerType, taskRawCapture.Data);
          //    captureActor.Tell(new ProcessPacketRequest(parsedPacket));
          //}).ConfigureAwait(false);

          //captureActor.Tell(new RawPacket(captureDevice.LinkType, taskRawCapture.Data));

          rawCapture = captureDevice.GetNextPacket();
        }
      }
      finally
      {
        captureDevice.Close();
      }
    }
  }
}
