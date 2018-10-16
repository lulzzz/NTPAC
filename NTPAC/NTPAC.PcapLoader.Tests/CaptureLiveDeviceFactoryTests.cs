using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using SharpPcap;
using Xunit;

namespace NTPAC.PcapLoader.Tests
{
  public class CaptureLiveDeviceFactoryTests
  {
    private CaptureLiveDeviceFactory _captureLiveDeviceFactorySUT;

    public CaptureLiveDeviceFactoryTests()
    {
      var serviceCollection = new ServiceCollection();
      this._captureLiveDeviceFactorySUT = new CaptureLiveDeviceFactory(serviceCollection);
    }

    [Fact]
    public void DeviceMode_Promiscuous()
    {
      //Assert
      Assert.Equal(DeviceMode.Promiscuous, this._captureLiveDeviceFactorySUT.DeviceMode);
    }

    [Fact]
    public void EmptyUri_CreateInstance_Exception()
    {
      //Act
      var uri = new Uri("rpcap://");

      //Arrange & Assert
      Assert.Throws<InvalidOperationException>(() => this._captureLiveDeviceFactorySUT.CreateInstance(uri));
    }

    [Fact]
    public void Existing_CreateInstance_ReturnsAdapter()
    {
      //Act
      //E.g., var deviceUri = @"rpcap://\Device\NPF_{8ED17681-57BA-4BB3-85C8-F483C7B13FA3}"; //invalid WinPcap URI...
      var deviceUri = CaptureDeviceList.Instance.First().Name;
      var uri       = new Uri(new String(deviceUri).Replace(@"\", @"/")); //Replace fixes invalid WinPcap URI.

      //Arrange
      var device = this._captureLiveDeviceFactorySUT.CreateInstance(uri);

      //Assert
      Assert.Equal(deviceUri, device.Name);
    }

    [Fact]
    public void InvalidUri_CreateInstance_Exception()
    {
      //Act
      var uri = new Uri("rpcap://BullShit");

      //Arrange & Assert
      Assert.Throws<InvalidOperationException>(() => this._captureLiveDeviceFactorySUT.CreateInstance(uri));
    }
  }
}
