using System;
using Akka.Configuration;
using Google.Protobuf.WellKnownTypes;

namespace NTPAC.LoadBalancer.Interfaces
{
  public class LoadBalancerSettings
  {
    public TimeSpan BatchFlushInterval { get; set; } = TimeSpan.FromSeconds(10);
    public Int32 BatchRawCapturesSizeLimitBytes { get; set; } = 50_000;
    public Int32 BatchSize { get; set; } = 5_000;
    public Int32 ParallelBatchTransmissionsPerReassembler { get; set; } = 500;
    public Int32 ShardsPerEntity { get; set; } = 100;
    public TimeSpan AskableMessageReplyTimeout { get; set; } = TimeSpan.FromSeconds(32);
    public TimeSpan PeriodicalGarbageCollectingInitial { get; set; } = TimeSpan.FromSeconds(120);
    public TimeSpan PeriodicalGarbageCollectingPeriod { get; set;} = TimeSpan.FromSeconds(120);

    public LoadBalancerSettings() { }
    
    public LoadBalancerSettings(Config config)
    {
      var loadBalancerConfigSection = config.GetConfig("ntpac-loadbalancer");
      if (loadBalancerConfigSection == null)
      {
        return;
      }

      this.BatchFlushInterval = 
        TimeSpan.FromSeconds(loadBalancerConfigSection.GetInt("batch-flush-interval", this.BatchFlushInterval.Seconds));
      this.BatchRawCapturesSizeLimitBytes =
        loadBalancerConfigSection.GetInt("batch-raw-capture-size", this.BatchRawCapturesSizeLimitBytes);
      this.BatchSize =
        loadBalancerConfigSection.GetInt("batch-size", this.BatchSize);
      this.ParallelBatchTransmissionsPerReassembler =
        loadBalancerConfigSection.GetInt("batch-parallel-transmissions-per-reassembler", this.ParallelBatchTransmissionsPerReassembler);
      this.ShardsPerEntity =
        loadBalancerConfigSection.GetInt("shards-per-entity", this.ShardsPerEntity);
      this.AskableMessageReplyTimeout =
        TimeSpan.FromSeconds(loadBalancerConfigSection.GetInt("askable-message-reply-timeout", this.AskableMessageReplyTimeout.Seconds));
      this.PeriodicalGarbageCollectingInitial =
        TimeSpan.FromSeconds(loadBalancerConfigSection.GetInt("gc-delay", this.PeriodicalGarbageCollectingInitial.Seconds));
      this.PeriodicalGarbageCollectingPeriod =
        TimeSpan.FromSeconds(loadBalancerConfigSection.GetInt("gc-interval", this.PeriodicalGarbageCollectingPeriod.Seconds));
    }
  }
}
