using System;
using MessagePack;
using NTPAC.AkkaSupport.Interfaces;

namespace NTPAC.Messages.RawPacket
{
  [MessagePackObject]
  public class RawPacketBatchAck : IAskableMessageReply
  {
    [Key(0)]
    public Int64 MessageId { get; set; }  
  }
}
