using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MessagePack;
using NTPAC.AkkaSupport.Interfaces;

namespace NTPAC.Messages.RawPacket
{
  [MessagePackObject]
  [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
  public class RawPacketBatchRequest : IAskableMessageRequest
  {
    [Key(0)] public IReadOnlyCollection<RawPacket> RawPackets;

    [Key(1)] public Int64 SeqId;
    
    [Key(2)]
    public Int64 MessageId { get; set; }

    public RawPacketBatchRequest(IReadOnlyCollection<RawPacket> rawPackets, Int64 seqId)
    {
      this.RawPackets = rawPackets;
      this.SeqId      = seqId;
    }
   
    public RawPacketBatchRequest() {}
  }
}
