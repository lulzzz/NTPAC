using System;

namespace NTPAC.Common.Interfaces
{
  public interface IProcessingResult
  {
    UInt64 ProcessedPackets { get; }
    TimeSpan DistributionTime { get; }
    Boolean Success { get; }
    TimeSpan TotalTime { get; }
    Int64? CaptureSize { get; }
    Int64? ProcessingThroughputBps { get; }
    Int64 ProcessingThroughputPps { get; }
    String ToString();
  }
}
