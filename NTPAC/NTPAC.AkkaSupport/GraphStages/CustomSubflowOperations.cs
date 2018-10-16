using System;
using System.Collections.Generic;
using Akka.Streams.Dsl;
using NTPAC.Messages.RawPacket;

namespace NTPAC.AkkaSupport.GraphStages
{
  public static class CustomSubflowOperations
  {
    public static SubFlow<IEnumerable<RawPacket>, TMat, TClosed> GroupedByCountAndRawCaptureSizeWithin<TMat, TClosed>(
      this SubFlow<RawPacket, TMat, TClosed> flow,
      Int32 count,
      Int32 rawCapturesSizeLimit,
      TimeSpan timeout) =>
      (SubFlow<IEnumerable<RawPacket>, TMat, TClosed>) flow.GroupedByCountAndRawCaptureSizeWithin<RawPacket, TMat>(
        count, rawCapturesSizeLimit, timeout);
  }
}
