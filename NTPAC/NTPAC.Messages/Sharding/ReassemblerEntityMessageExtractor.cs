using System;
using Akka.Cluster.Sharding;

namespace NTPAC.Messages.Sharding
{
  public class ReassemblerEntityMessageExtractor : IMessageExtractor
  {
    public String EntityId(Object message) => ((ReassemblerEntityMessageEnvelope) message).EntityId.ToString();

    public Object EntityMessage(Object message) => ((ReassemblerEntityMessageEnvelope) message).Message;

    public String ShardId(Object message) => ((ReassemblerEntityMessageEnvelope) message).EntityId.ToString();
  }
}
