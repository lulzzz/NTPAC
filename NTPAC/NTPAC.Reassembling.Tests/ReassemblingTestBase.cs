using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.XunitLogger;
using NTPAC.Common.Extensions;
using NTPAC.Common.Interfaces;
using NTPAC.ConversatinTracking.Interfaces.Models;
using NTPAC.ConversationTracking.Factories;
using NTPAC.ConversationTracking.Helpers;
using NTPAC.PcapLoader;
using NTPAC.Reassembling.IP;
using PacketDotNet;
using SharpPcap;
using Xunit.Abstractions;

namespace NTPAC.Reassembling.Tests
{
  public abstract class ReassemblingTestBase
  {
    protected static readonly String TestPcapsDir = @"..\..\..\..\TestingData";

    private readonly ServiceProvider _services;

    protected ReassemblingTestBase(ITestOutputHelper output)
    {
      IServiceCollection serviceCollection = new ServiceCollection();
      ConfigureServices(serviceCollection);
      this._services = serviceCollection.BuildServiceProvider();
      this._services.GetService<ILoggerFactory>().AddProvider(new XunitLoggerProvider(LogLevel.Debug, output));
    }

    protected IEnumerable<Frame> GetFramesFromPcap(String pcapFileName, Boolean defragment = false)
    {
      var fileInfo = new FileInfo(TestPcapPath(pcapFileName));
      var baseUri  = fileInfo.ToUri();

      return defragment ? this.GetFramesFromPcapAndDefragment(baseUri) : this.GetFramesFromPcap(baseUri);
    }

    // TODO extract to project
    protected IEnumerable<Frame> GetFramesFromPcap(Uri pcapUri)
    {
      var frames     = new List<Frame>();
      var pcapLoader = this._services.GetService<IPcapLoader>();
      using (pcapLoader)
      {
        RawCapture rawCapture;
        pcapLoader.Open(pcapUri);
        while ((rawCapture = pcapLoader.GetNextPacket()) != null)
        {
          var packet = Packet.ParsePacket(pcapLoader.LinkType, rawCapture.Data);
          var frame  = FrameFactory.CreateFromPacket(packet, rawCapture.Timeval.Date.Ticks);
          frames.Add(frame);
        }
      }

      return frames;
    }

    protected IEnumerable<Frame> GetFramesFromPcapAndDefragment(Uri pcapUri)
    {
      var ipv4DefragmentationEngine =
        new Ipv4DefragmentationEngine(this._services.GetService<ILoggerFactory>().CreateLogger<Ipv4DefragmentationEngine>());
      var frames     = new List<Frame>();
      var pcapLoader = this._services.GetService<IPcapLoader>();
      using (pcapLoader)
      {
        RawCapture rawCapture;
        pcapLoader.Open(pcapUri);
        while ((rawCapture = pcapLoader.GetNextPacket()) != null)
        {
          Frame frame;
          var   parsedPacket = Packet.ParsePacket(pcapLoader.LinkType, rawCapture.Data);
          if (!(parsedPacket.PayloadPacket is IPPacket ipPacket))
          {
            continue;
            ;
          }

          if (ipPacket is IPv4Packet ipv4Packet && Ipv4Helpers.Ipv4PacketIsFragmented(ipv4Packet))
          {
            var (isDefragmentationSuccessful, firstTimeStamp, defragmentedIpv4Packet) =
              ipv4DefragmentationEngine.TryDefragmentFragments(
                FrameFactory.CreateFromIpPacket(ipv4Packet, rawCapture.Timeval.Date.Ticks));
            if (!isDefragmentationSuccessful)
            {
              continue;
            }

            frame = FrameFactory.CreateFromIpPacket(defragmentedIpv4Packet, firstTimeStamp);
          }
          else
          {
            frame = FrameFactory.CreateFromPacket(parsedPacket, rawCapture.Timeval.Date.Ticks);
          }

          frames.Add(frame);
        }
      }

      return frames;
    }


    private static void ConfigureServices(IServiceCollection serviceCollection)
    {
      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(serviceCollection);
      serviceCollection.AddSingleton<IPcapLoader, PcapLoader.PcapLoader>();
      serviceCollection.AddSingleton<ICaptureDeviceFactory, CaptureDeviceFactory>();
    }

    private static String TestPcapPath(String pcapFileName) => Path.Combine(TestPcapsDir, pcapFileName);
  }
}
