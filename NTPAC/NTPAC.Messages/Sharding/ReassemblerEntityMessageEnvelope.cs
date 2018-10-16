using System;
using MessagePack;
using NTPAC.AkkaSupport.Interfaces;

namespace NTPAC.Messages.Sharding
{
  [MessagePackObject]
  public class ReassemblerEntityMessageEnvelope
  {
    [Key(0)] public Int32 EntityId;

    [Key(1)] public Object Message;

    public ReassemblerEntityMessageEnvelope(Int32 entityId, IAskableMessageRequest message)
    {
      this.EntityId = entityId;
      this.Message  = message;
    }

    public ReassemblerEntityMessageEnvelope()
    {
      
    }
  }
}
