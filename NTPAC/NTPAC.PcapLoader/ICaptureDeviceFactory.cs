using System;
using SharpPcap;

namespace NTPAC.PcapLoader
{
  public interface ICaptureDeviceFactory
  {
    DeviceMode? DeviceMode { get; }

    /// <summary>
    ///   Creates device instance.
    /// </summary>
    /// <param name="uri"></param>
    /// <exception cref="InvalidOperationException">In case that device is unavailable.</exception>
    /// <returns></returns>
    ICaptureDevice CreateInstance(Uri uri);
  }
}
