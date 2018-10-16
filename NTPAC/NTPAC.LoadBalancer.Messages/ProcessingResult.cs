using System;
using System.Text;
using NTPAC.Common.Interfaces;

namespace NTPAC.LoadBalancer.Messages
{
  public class ProcessingResult : IProcessingResult
  {
    public UInt64 ProcessedPackets { get; set; }
    public TimeSpan DistributionTime { get; set; }
    public Boolean Success { get; set; }
    public TimeSpan TotalTime { get; set; }
    public Int64? CaptureSize { get; set; }
    public Exception Exception { get; set; }

    public Int64? ProcessingThroughputBps =>
      this.CaptureSize.HasValue
        ? (Int64) Math.Round((this.CaptureSize.Value * 8) / (this.TotalTime.TotalSeconds))
        : (Int64?) null;

    public Int64 ProcessingThroughputPps =>
      this.ProcessedPackets != 0 ? (Int64) Math.Round(this.ProcessedPackets / this.TotalTime.TotalSeconds) : 0;

    public override String ToString()
    {
      var sb = new StringBuilder();
      sb.AppendLine($"Processing success: {this.Success}");
      if (this.Exception != null)
      {
        sb.AppendLine($@"Exception message: {this.Exception.Message}
Exception stack trace:");
        sb.Append(this.Exception.StackTrace);
      }
      else
      {
        sb.Append($@"Distribution time: {this.DistributionTime}, Total time: {this.TotalTime}
Capture size: {this.CaptureSize.GetValueOrDefault()} B, {this.ProcessingThroughputBps.GetValueOrDefault() / 1_000_000} Mb/s
Processed packets: {this.ProcessedPackets}, {this.ProcessingThroughputPps} pkt/s");
      }
      return sb.ToString();
    } 
  }
}
