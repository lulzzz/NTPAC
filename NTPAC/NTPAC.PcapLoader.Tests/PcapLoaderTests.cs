using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions.Internal;
using Moq;
using PacketDotNet;
using SharpPcap;
using Xunit;

namespace NTPAC.PcapLoader.Tests
{
  public class PcapLoaderTests
  {
    private readonly Mock<ICaptureDeviceFactory> _captureDeviceFactoryMock;
    private readonly Mock<ICaptureDevice> _captureDeviceMock;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly PcapLoader _pcapLoader;

    public PcapLoaderTests()
    {
      this._loggerFactoryMock = new Mock<ILoggerFactory>();
      this._loggerFactoryMock.Setup(p => p.CreateLogger(It.IsAny<String>())).Returns(new Mock<ILogger<PcapLoader>>().Object);

      this._captureDeviceMock = new Mock<ICaptureDevice>();
      this._captureDeviceMock.SetupGet(device => device.LinkType).Returns(LinkLayers.AmateurRadioAX25);

      this._captureDeviceFactoryMock = new Mock<ICaptureDeviceFactory>();
      this._captureDeviceFactoryMock.Setup(factory => factory.CreateInstance(It.IsAny<Uri>()))
          .Returns(this._captureDeviceMock.Object);

      this._pcapLoader = new PcapLoader(this._loggerFactoryMock.Object, this._captureDeviceFactoryMock.Object);
    }

    [Fact]
    public void Closed_GetNextPacket() { Assert.Throws<InvalidOperationException>(() => this._pcapLoader.GetNextPacket()); }

    [Fact]
    public void Closed_Open_Opened()
    {
      //Act
      this._pcapLoader.Open(new Uri("smd://test"));

      //Assert
      this._captureDeviceFactoryMock.Verify(m => m.CreateInstance(It.IsAny<Uri>()));
    }

    [Fact]
    public void ClosedDevice_LinkType() { Assert.Equal(LinkLayers.Null, this._pcapLoader.LinkType); }

    [Fact]
    public void Ctor()
    {
      this._loggerFactoryMock.Verify(factory => factory.CreateLogger(TypeNameHelper.GetTypeDisplayName(typeof(PcapLoader))),
                                     Times.Once);
    }

    [Fact]
    public void Dispose()
    {
      //Act
      this._pcapLoader.Dispose();
    }

    [Fact]
    public void Opened_Close()
    {
      //Arrange
      this._pcapLoader.Open(new Uri("smd://test"));

      //Act
      this._pcapLoader.Close();

      //Assert
      this._captureDeviceMock.Verify(d => d.Close());
    }

    [Fact]
    public void Opened_GetNextPacket()
    {
      //Arrange
      this._pcapLoader.Open(new Uri("smd://test"));

      //Act
      this._pcapLoader.GetNextPacket();

      //Assert
      this._captureDeviceMock.Verify(m => m.GetNextPacket(), Times.Once);
    }

    [Fact]
    public void OpenedDevice_LinkType()
    {
      //Arrange
      this._pcapLoader.Open(new Uri("smd://test"));

      Assert.Equal(LinkLayers.AmateurRadioAX25, this._pcapLoader.LinkType);
    }

    [Fact]
    public void OpenedDevice_Open()
    {
      //Arrange
      this._pcapLoader.Open(new Uri("smd://test"));
      var uri = new Uri("smd://test");

      Assert.Throws<InvalidOperationException>(() => this._pcapLoader.Open(uri));
    }
  }
}
