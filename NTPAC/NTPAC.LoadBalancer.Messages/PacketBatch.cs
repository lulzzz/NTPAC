using System;
using System.Collections.Generic;
using NTPAC.Messages.RawPacket;

namespace NTPAC.LoadBalancer.Messages
{
  public class PacketBatch
  {
    private readonly List<RawPacket> _batch;
    public PacketBatch(Int32 batchSize) => this._batch = new List<RawPacket>(batchSize);
    public IReadOnlyCollection<RawPacket> Batch => this._batch;
    public Int32 Capacity => this._batch.Capacity;
    public Int32 Count => this._batch.Count;
    public Boolean IsEmpty => this._batch.Count == 0;
    public void Add(RawPacket rawPacket) { this._batch.Add(rawPacket); }
  }
}
