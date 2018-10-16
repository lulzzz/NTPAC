using System;
using System.Collections.Generic;
using Akka;
using Akka.Streams;
using Akka.Streams.Dsl;
using NTPAC.Common.Interfaces;

namespace NTPAC.AkkaSupport.GraphStages
{
  public static class CustomFlowOperations
  {
    public static IFlow<IEnumerable<TProcessRawPacketRequest>, TMat>
      GroupedByCountAndRawCaptureSizeWithin<TProcessRawPacketRequest, TMat>(this IFlow<TProcessRawPacketRequest, TMat> flow,
                                                                           Int32 count,
                                                                           Int32 rawCapturesSizeLimit,
                                                                           TimeSpan timeout)
    {
      if (count <= 0)
        throw new ArgumentException("count must be > 0", nameof(count));
      if (rawCapturesSizeLimit <= 0)
        throw new ArgumentException("RawCapturesSizeLimit must be > 0", nameof(rawCapturesSizeLimit));
      if (timeout == TimeSpan.Zero)
        throw new ArgumentException("Timeout must be non-zero", nameof(timeout));
      return flow.Via(
        (IGraph<FlowShape<TProcessRawPacketRequest, IEnumerable<TProcessRawPacketRequest>>, NotUsed>)
        new GroupedByCountAndRawCaptureSizeWithin(count, rawCapturesSizeLimit, timeout));
    }

    public static Source<T, TMat> MaybeSelectMultipleValues<T, TMat>(this Source<IMaybeMultipleValues<T>, TMat> flow)
      where T : class =>
      flow.Via(new MaybeSelectMultipleValues<T>());
  }
}
