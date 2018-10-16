using System;
using System.Threading.Tasks;

namespace NTPAC.Common.Interfaces
{
  public interface IPacketIngestor
  {
    IProcessingResult OpenCapture(Uri uri);
    Task<IProcessingResult> OpenCaptureAsync(Uri uri);
  }
}
