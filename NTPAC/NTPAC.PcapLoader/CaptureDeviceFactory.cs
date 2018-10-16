using System;
using Microsoft.Extensions.DependencyInjection;
using SharpPcap;
using SharpPcap.LibPcap;

namespace NTPAC.PcapLoader
{
  public class CaptureDeviceFactory : ICaptureDeviceFactory
  {
    private readonly IServiceCollection _services;

    public CaptureDeviceFactory(IServiceCollection services) => this._services = services;
    public DeviceMode? DeviceMode { get; } = null;

    public ICaptureDevice CreateInstance(Uri uri) => new CaptureFileReaderDevice(uri.AbsolutePath);
  }
}
