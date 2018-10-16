using System;
using Akka.Actor;
using Akka.Configuration;
using Akka.Dispatch;
using NTPAC.Messages.RawPacket;

namespace NTPAC.ConversationTracking.Actors.Mailboxes
{
  public class RawPacketBatchRequestPriorityMailbox : UnboundedPriorityMailbox
  {
    public RawPacketBatchRequestPriorityMailbox(Settings settings, Config config) : base(settings, config)
    {
    }

    protected override Int32 PriorityGenerator(Object message)
    {
      switch (message)
      {
        case RawPacketBatchRequest _:
          return 0;
        default:
          return 1;
      }
    }
  }
}
