using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using SharpPcap;

namespace NTPAC.PcapLoader
{
  public class CaptureLiveDeviceFactory : ICaptureDeviceFactory
  {
    private readonly IServiceCollection _services;

    public CaptureLiveDeviceFactory(IServiceCollection services) => this._services = services;

    public DeviceMode? DeviceMode { get; } = SharpPcap.DeviceMode.Promiscuous;

    private static Uri MapDeviceToUriUri(ICaptureDevice captureDevice)
    {
      try
      {
        return new Uri(captureDevice.Name);
      }
      catch (UriFormatException)
      {
        try
        {
          //To fix WinPcap invalid URI implementation.
          var reverseSlash = captureDevice.Name.Replace(@"\", @"/");
          return new Uri(reverseSlash);
        }
        catch (UriFormatException)
        {
          return null;
        }
      }
    }

    /// <summary>
    ///   Creates device instance.
    /// </summary>
    /// <param name="uri"></param>
    /// <exception cref="InvalidOperationException">In case that device is unavailable.</exception>
    /// <returns></returns>
    public ICaptureDevice CreateInstance(Uri uri)
    {
      var devices = CaptureDeviceList.Instance;
      var device  = devices.FirstOrDefault(d => MapDeviceToUriUri(d)?.LocalPath == uri.LocalPath);

      if (device != null)
      {
        return device;
      }

      var deviceNames = CaptureDeviceList.Instance.Select(i => $"{i.Name}, {i.Description}{Environment.NewLine}");
      var devicesList = String.Join(", ", deviceNames);
      throw new InvalidOperationException($"There is no such device: {uri.LocalPath}. Use: {devicesList}");
    }
  }
}
