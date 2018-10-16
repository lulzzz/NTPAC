using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.Xunit2;
using Moq;
using NTPAC.ConversatinTracking.Interfaces;
using NTPAC.ConversatinTracking.Interfaces.Models;
using NTPAC.LoadBalancer.Interfaces;
using NTPAC.LoadBalancer.Messages;
using NTPAC.Messages.CaptureTracking;
using NTPAC.Messages.RawPacket;
using Xunit;

namespace NTPAC.LoadBalancer.Actors.Tests
{
  public sealed class OfflineLoadBalancerActorTests : TestKit
  {
    private readonly Mock<IBatchLoader> _batchLoaderMock;
    private readonly Mock<IBatchSender> _batchSenderMock;
    private readonly LoadBalancerSettings _loadBalancerSettings;
    private readonly IActorRef _offlineLoadBalancerActorRefSUT;
    private readonly TestProbe _rawPacketBatchActorTestProbe;
    private readonly Mock<IRawPacketBatchParserActorFactory> _rawPacketBatchParserFactoryMock;
    
    private static readonly Int64 MessageId = 10;

    public OfflineLoadBalancerActorTests()
    {
      this._loadBalancerSettings = new LoadBalancerSettings();

      this._rawPacketBatchActorTestProbe = this.CreateTestProbe("RawPacketBatch");

      this._batchLoaderMock                 = new Mock<IBatchLoader>();
      this._batchSenderMock                 = new Mock<IBatchSender>();
      this._rawPacketBatchParserFactoryMock = new Mock<IRawPacketBatchParserActorFactory>();
      this._rawPacketBatchParserFactoryMock
          .Setup(factory => factory.Create(It.IsAny<IActorContext>(), It.IsAny<IActorRef>(), It.IsAny<CaptureInfo>()))
          .Returns(this._rawPacketBatchActorTestProbe);


      this._offlineLoadBalancerActorRefSUT =
        this.Sys.ActorOf(Props.Create(() => new OfflineLoadBalancerActor(this._batchLoaderMock.Object,
                                                                         this._batchSenderMock.Object, this._loadBalancerSettings,
                                                                         this._rawPacketBatchParserFactoryMock.Object)));

      this._batchSenderMock.Setup(sender => sender.Complete())
          .Callback(() => this._offlineLoadBalancerActorRefSUT.Tell(new CaptureTrackingCompleted()));
    }

    [Fact]
    public async Task LoadBatch_DataLoaded_Send_Acknowledged_SendFinish()
    {
      //Arrange
      var uri                      = new Uri(@"test://");
      var packetBatchWithOnePacker = new PacketBatch(this._loadBalancerSettings.BatchSize);
      packetBatchWithOnePacker.Add(new RawPacket());

      var packetBatchEmpty = new PacketBatch(this._loadBalancerSettings.BatchSize);


      this._batchLoaderMock.Setup(loader => loader.LoadBatch(this._loadBalancerSettings.BatchSize))
          .Returns(() => Task.Run(() => packetBatchWithOnePacker));

      this._batchSenderMock.Setup(sender => sender.DistributedPackets).Returns(1);

      //Act
      this._offlineLoadBalancerActorRefSUT.Tell(new CaptureProcessingRequest {CaptureInfo = new CaptureInfo(uri)});
      await Task.Delay(50);

      this._batchLoaderMock.Setup(loader => loader.LoadBatch(this._loadBalancerSettings.BatchSize))
          .Returns(() => Task.Run(() => packetBatchEmpty));

      this._offlineLoadBalancerActorRefSUT.Tell(new RawPacketBatchAck());

      //Assert
      await Task.Delay(50);

      this._batchLoaderMock.Verify(loader => loader.LoadBatch(this._loadBalancerSettings.BatchSize), Times.Exactly(2));
      this._batchSenderMock.Verify(sender => sender.SendBatch(packetBatchWithOnePacker), Times.Once);

      var processingResult = this.ExpectMsg<ProcessingResult>();
      Assert.True(processingResult.Success);
      Assert.Equal((UInt64) 1, processingResult.ProcessedPackets);
    }

    [Fact]
    public async Task LoadBatch_DataLoaded_SendPacketBatch()
    {
      //Arrange
      var uri         = new Uri(@"test://");
      var packetBatch = new PacketBatch(this._loadBalancerSettings.BatchSize);
      packetBatch.Add(new RawPacket());

      this._batchLoaderMock.Setup(loader => loader.LoadBatch(this._loadBalancerSettings.BatchSize))
          .Returns(() => Task.Run(() => packetBatch));

      //Act
      this._offlineLoadBalancerActorRefSUT.Tell(new CaptureProcessingRequest {CaptureInfo = new CaptureInfo(uri)});

      //Assert
      await Task.Delay(50);
      this._batchLoaderMock.Verify(loader => loader.LoadBatch(this._loadBalancerSettings.BatchSize), Times.Once);
      this._batchSenderMock.Verify(sender => sender.SendBatch(packetBatch), Times.Once);
    }

    [Fact]
    public async Task LoadBatch_DataLoadedEmpty_SendFinish()
    {
      //Arrange
      var uri         = new Uri(@"test://");
      var packetBatch = new PacketBatch(this._loadBalancerSettings.BatchSize);

      this._batchLoaderMock.Setup(loader => loader.LoadBatch(this._loadBalancerSettings.BatchSize))
          .Returns(() => Task.Run(() => packetBatch));

      //Act
      this._offlineLoadBalancerActorRefSUT.Tell(new CaptureProcessingRequest {CaptureInfo = new CaptureInfo(uri)});

      //Assert
      await Task.Delay(50);
      this._batchLoaderMock.Verify(loader => loader.LoadBatch(this._loadBalancerSettings.BatchSize), Times.Once);
      this._batchSenderMock.Verify(sender => sender.SendBatch(packetBatch), Times.Never);

      var processingResult = this.ExpectMsg<ProcessingResult>();
      Assert.True(processingResult.Success);
      Assert.Equal((UInt64) 0, processingResult.ProcessedPackets);
    }

    [Fact]
    public async Task RunGarbageCollecting()
    {
      //Arrange
      var uri = new Uri(@"test://");

      //Act
      this._offlineLoadBalancerActorRefSUT.Tell(CollectGarbageRequest.Instance);

      //Assert
      await Task.Delay(50);
      this._batchSenderMock.Verify(
        sender => sender.Initialize(this._offlineLoadBalancerActorRefSUT, this._rawPacketBatchActorTestProbe), Times.Never);
      this._batchLoaderMock.Verify(loader => loader.Open(uri), Times.Never);
      this._batchLoaderMock.Verify(loader => loader.LoadBatch(It.IsAny<Int32>()), Times.Never);
    }

    [Fact]
    public async Task Send_StartProcessingRequest_StartProcessingInitiated()
    {
      //Arrange

      var uri                      = new Uri(@"test://");
      var captureProcessingRequest = new CaptureProcessingRequest {CaptureInfo = new CaptureInfo(uri)};

      //Act
      this._offlineLoadBalancerActorRefSUT.Tell(captureProcessingRequest);

      //Assert
      await Task.Delay(50);
      this._rawPacketBatchParserFactoryMock.Verify(
        factory => factory.Create(It.IsAny<IActorContext>(), It.IsAny<IActorRef>(), It.IsAny<CaptureInfo>()), Times.Once);
      this._batchSenderMock.Verify(
        sender => sender.Initialize(this._offlineLoadBalancerActorRefSUT, this._rawPacketBatchActorTestProbe), Times.Once);
      this._batchLoaderMock.Verify(loader => loader.Open(uri), Times.Once);
      this._batchLoaderMock.Verify(loader => loader.LoadBatch(It.IsAny<Int32>()), Times.Once);
    }

    [Fact]
    public async Task UnexpectedMessage_During_Finalizing_Handled()
    {
      //Arrange
      var uri         = new Uri(@"test://");
      var packetBatch = new PacketBatch(this._loadBalancerSettings.BatchSize);

      this._batchLoaderMock.Setup(loader => loader.LoadBatch(this._loadBalancerSettings.BatchSize))
          .Returns(() => Task.Run(() => packetBatch));

      //Act
      this._offlineLoadBalancerActorRefSUT.Tell(new CaptureProcessingRequest {CaptureInfo = new CaptureInfo(uri)});
      await Task.Delay(50);
      this._offlineLoadBalancerActorRefSUT.Tell(new RawPacketBatchAck());

      //Assert
      await Task.Delay(50);

      var processingResult = this.ExpectMsg<ProcessingResult>();
      Assert.True(processingResult.Success);
      Assert.Equal((UInt64) 0, processingResult.ProcessedPackets);
    }

    [Fact]
    public async Task UnexpectedMessage_During_StartProcessing_Handled()
    {
      //Arrange
      var uri         = new Uri(@"test://");
      var packetBatch = new PacketBatch(this._loadBalancerSettings.BatchSize);
      packetBatch.Add(new RawPacket());

      this._batchLoaderMock.Setup(loader => loader.LoadBatch(this._loadBalancerSettings.BatchSize))
          .Returns(() => Task.Run(() => packetBatch));

      this._offlineLoadBalancerActorRefSUT.Tell(new CaptureProcessingRequest {CaptureInfo = new CaptureInfo(uri)});
      await Task.Delay(50);

      //Act
      this._offlineLoadBalancerActorRefSUT.Tell(new CaptureProcessingRequest {CaptureInfo = new CaptureInfo(uri)});

      //Assert
      await Task.Delay(50);
      this._batchLoaderMock.Verify(loader => loader.LoadBatch(this._loadBalancerSettings.BatchSize), Times.Once);
      this._batchSenderMock.Verify(sender => sender.SendBatch(packetBatch), Times.Once);
      //TODO test that error message was logged
    }
  }
}
