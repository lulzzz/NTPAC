using System;
using System.IO;
using Akka.Remote;
using Microsoft.Extensions.Logging;
using NTPAC.Common.Interfaces;
using PacketDotNet;
using SharpPcap;

namespace NTPAC.PcapLoader
{
  public class PcapLoader : IDisposable, IPcapLoader
  {
    private readonly ICaptureDeviceFactory _captureDeviceFactory;
    private readonly ILogger _logger;
    private ICaptureDevice _captureDevice;
    private LinkLayers? _linkType;
    private Uri _uri;
    private Int64? _captureSize;

    public PcapLoader(ILoggerFactory loggerFactory, ICaptureDeviceFactory captureDeviceFactory)
    {
      this._captureDeviceFactory = captureDeviceFactory;
      this._logger               = loggerFactory.CreateLogger<PcapLoader>();
    }

    public LinkLayers LinkType => (this._linkType ?? (this._linkType = this._captureDevice?.LinkType ?? LinkLayers.Null)).Value;

    public void Dispose() { 
      this._captureDevice?.Close();
      this._uri = null;
      this._linkType = null;
    }

    public void Close() { this.Dispose(); }

    public RawCapture GetNextPacket()
    {
      try
      {
        return this._captureDevice.GetNextPacket();
      }
      catch (NullReferenceException)
      {
        throw new InvalidOperationException("Capture device needs to be opened first.");
      }
    }

    public void Open(Uri uri)
    { 
      if (this._captureDevice != null)
      {
        throw new InvalidOperationException("Only one call per object.");
      }

      this._uri = uri;
      
      // Capture file, not live interface
      if (this._uri.IsFile)
      {
         this._captureSize = new FileInfo(this._uri.AbsolutePath).Length;
      }

      this._captureDevice = this._captureDeviceFactory.CreateInstance(this._uri);

      if (this._captureDevice == null)
      {
        throw new InvalidOperationException($"There is no such device: {this._uri.AbsoluteUri}");
      }

      this._logger.LogInformation($"Opening> {this._uri}");

      if (this._captureDeviceFactory.DeviceMode == null)
      {
        this._captureDevice.Open();
      }
      else
      {
        this._captureDevice.Open((DeviceMode) this._captureDeviceFactory.DeviceMode);
      }
    }

    public Int64? CaptureSize => this._captureSize;
  }
}
