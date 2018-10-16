using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.Xunit2;
using Moq;
using NTPAC.ConversatinTracking.Interfaces;
using NTPAC.ConversatinTracking.Interfaces.Models;
using NTPAC.Messages.CaptureTracking;
using NTPAC.Messages.RawPacket;
using PacketDotNet;
using SharpPcap;
using Xunit;

namespace NTPAC.ConversationTracking.Actors.Tests
{
  public sealed class RawPacketBatchParserActorTests : TestKit
  {
    private readonly TestProbe _captureActorTestProbe;
    private readonly Mock<ICaptureTrackingActorFactory> _captureTrackingActorFactoryMock;
    private readonly TestProbe _loadBalancerActorTestProbe;
    private readonly IActorRef _rawPacketBatchParserActorRefSUT;
    
    private static readonly Int64 MessageId = 10;

    public RawPacketBatchParserActorTests()
    {
      var uri         = new Uri(@"test://");
      var captureInfo = new CaptureInfo(uri);

      this._loadBalancerActorTestProbe = this.CreateTestProbe("LoadBalancerActor");
      this._captureActorTestProbe      = this.CreateTestProbe("CaptureTrackingActor");

      this._captureTrackingActorFactoryMock = new Mock<ICaptureTrackingActorFactory>();
      this._captureTrackingActorFactoryMock
          .Setup(factory => factory.Create(It.IsAny<IActorContext>(), captureInfo, It.IsAny<IActorRef>()))
          .Returns(this._captureActorTestProbe);

      this._rawPacketBatchParserActorRefSUT =
        this.Sys.ActorOf(Props.Create(() => new RawPacketBatchParserActor(this._captureTrackingActorFactoryMock.Object,
                                                                          this._loadBalancerActorTestProbe, captureInfo)));
    }

    [Fact]
    public void CaptureTrackingCompleted_RawPacketBatchCompleted()
    {
      //Arrange
      var completed = new CaptureTrackingCompleted() {MessageId = MessageId};

      //Act
      this._rawPacketBatchParserActorRefSUT.Tell(completed);

      //Assert
      this._loadBalancerActorTestProbe.ExpectMsg(completed);
    }

    [Fact]
    public async Task Instantiate()
    {
      //Arrange

      //Act

      //Assert
      await Task.Delay(50);
      this._captureTrackingActorFactoryMock.Verify(
        factory => factory.Create(It.IsAny<IActorContext>(), It.IsAny<CaptureInfo>(), this._rawPacketBatchParserActorRefSUT),
        Times.Once);
    }

    [Fact]
    public void Process_PacketBatch_NonTransportPacket()
    {
      var randomIPPacket = IPPacket.RandomPacket(IPVersion.IPv4);
      //Arrange
      var posixTimeval = new PosixTimeval();
      var rawCapture   = new RawCapture(LinkLayers.Raw, posixTimeval, randomIPPacket.Bytes);

      var rawPacketBatchRequest = new RawPacketBatchRequest(new[] {new RawPacket(rawCapture)}, 1);
      //Act
      this._rawPacketBatchParserActorRefSUT.Tell(rawPacketBatchRequest);

      //Assert
      this._captureActorTestProbe.ExpectNoMsg();
    }


    [Fact]
    public void Process_PacketBatch_OnePacket()
    {
      var randomIPPacket  = IPPacket.RandomPacket(IPVersion.IPv4);
      var randomTCPPacket = TcpPacket.RandomPacket();

      randomIPPacket.PayloadPacket = randomTCPPacket;
      //Arrange
      var posixTimeval = new PosixTimeval();
      var rawCapture   = new RawCapture(LinkLayers.Raw, posixTimeval, randomIPPacket.Bytes);

      var rawPacketBatchRequest = new RawPacketBatchRequest(new[] {new RawPacket(rawCapture)}, 1);
      //Act
      this._rawPacketBatchParserActorRefSUT.Tell(rawPacketBatchRequest);

      //Assert
      this._captureActorTestProbe.ExpectMsg<Frame>(frame => frame.TimestampTicks == posixTimeval.Date.Ticks);
      this._captureActorTestProbe.ExpectNoMsg();
    }

    [Fact]
    public void Process_PacketBatch_TwoPacket()
    {
      var randomIPNonTcpPacket = IPPacket.RandomPacket(IPVersion.IPv4);

      var randomIPPacket  = IPPacket.RandomPacket(IPVersion.IPv4);
      var randomTCPPacket = TcpPacket.RandomPacket();

      randomIPPacket.PayloadPacket = randomTCPPacket;
      //Arrange
      var posixTimeval  = new PosixTimeval();
      var rawTCPCapture = new RawCapture(LinkLayers.Raw, posixTimeval, randomIPPacket.Bytes);

      var rawNonTCPCapture = new RawCapture(LinkLayers.Raw, posixTimeval, randomIPNonTcpPacket.Bytes);

      var rawPacketBatchRequest =
        new RawPacketBatchRequest(
          new[] {new RawPacket(rawTCPCapture), new RawPacket(rawNonTCPCapture), new RawPacket(rawTCPCapture)}, 1);
      //Act
      this._rawPacketBatchParserActorRefSUT.Tell(rawPacketBatchRequest);

      //Assert
      this._captureActorTestProbe.ExpectMsg<Frame>(frame => frame.TimestampTicks == posixTimeval.Date.Ticks);
      this._captureActorTestProbe.ExpectMsg<Frame>(frame => frame.TimestampTicks == posixTimeval.Date.Ticks);
      this._captureActorTestProbe.ExpectNoMsg();
    }

    [Fact]
    public void RawPacketBatchComplete_CatureTrackingComplete()
    {
      //Arrange
      var complete = new CaptureTrackingComplete {MessageId = MessageId};

      //Act
      this._rawPacketBatchParserActorRefSUT.Tell(complete);

      //Assert
      this._captureActorTestProbe.ExpectMsg(complete);
    }
  }
}
