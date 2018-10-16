using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster.Sharding;

namespace NTPAC.AkkaSupport
{
  public class LeastPacketsAllocationStrategy : IShardAllocationStrategy
  {
    private readonly ConcurrentDictionary<String, Int64> _shardSentPackets = new ConcurrentDictionary<String, Int64>();

    public Int64 UpadateShardPacketsRecord(String shardId, Int64 sentPackets) =>
      this._shardSentPackets.AddOrUpdate(shardId, sentPackets, (_, totalPackets) => totalPackets + sentPackets);

    private static T GetMinBy<T>(IEnumerable<T> collection, Func<T, Int64> extractor)
    {
      var minSize = Int64.MaxValue;
      var result  = default(T);
      foreach (var value in collection)
      {
        var x = extractor(value);
        if (x < minSize)
        {
          minSize = x;
          result  = value;
        }
      }

      return result;
    }


    private Int64 GetShardRegionPackets(IImmutableList<String> shardRegion)
    {
      Int64 shardRegionPackets = 0;
      foreach (var shardId in shardRegion)
      {
        if (this._shardSentPackets.TryGetValue(shardId, out var shardPackets))
        {
          shardRegionPackets += shardPackets;
        }
      }

      return shardRegionPackets;
    }

    public Task<IActorRef> AllocateShard(IActorRef requester,
                                         String shardId,
                                         IImmutableDictionary<IActorRef, IImmutableList<String>> currentShardAllocations)
    {
      var min = GetMinBy(currentShardAllocations, kv => this.GetShardRegionPackets(kv.Value));
      return Task.FromResult(min.Key);
    }

    public Task<IImmutableSet<String>> Rebalance(IImmutableDictionary<IActorRef, IImmutableList<String>> currentShardAllocations,
                                                 IImmutableSet<String> rebalanceInProgress) =>
      Task.FromResult<IImmutableSet<String>>(ImmutableHashSet<String>.Empty);
  }
}
