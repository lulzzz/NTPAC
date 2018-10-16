using NTPAC.Common.Interfaces;
using NTPAC.ConversatinTracking.Interfaces.Models;

namespace NTPAC.Actors.BenchmarkImplementations
{
  public class BenchmarkRequest
  {
    public static readonly BenchmarkRequest Instance = new BenchmarkRequest();
    public CaptureInfo CaptureInfo { get; set; }

    public IPcapLoader PcapLoader { get; set; }
  }
}
