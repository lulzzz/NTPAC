using System;
using System.Collections.Generic;
using System.Net;
using NTPAC.ConversatinTracking.Interfaces.Models;
using PacketDotNet;

namespace NTPAC.Reassembling.UDP
{
  public class UdpConversationTracker : L7ConversationTrackerBase
  {
    private static readonly Int64 UDPSessionAliveTimeoutTicks = TimeSpan.FromMinutes(10).Ticks;

    private L7Flow _downFlow;
    private L7Flow _upFlow;

    public UdpConversationTracker(IPEndPoint sourceEndPoint, IPEndPoint destinationEndPoint) : base(
      sourceEndPoint, destinationEndPoint)
    {
      this.ResetFlows();
    }

    public override IPProtocolType ProtocolType => IPProtocolType.UDP;

    public override IEnumerable<L7Conversation> Complete()
    {
      var newL7Conversation = this.CloseConversation();
      return newL7Conversation != null ? new List<L7Conversation> {newL7Conversation} : null;
    }

    public override L7Conversation ProcessFrame(Frame frame)
    {
      L7Conversation newL7Conversation = null;
      var            flowsLastSeen     = this.FlowsLastSeen();
      if (flowsLastSeen.HasValue && (frame.TimestampTicks - flowsLastSeen.Value) > UDPSessionAliveTimeoutTicks)
      {
        newL7Conversation = this.CloseConversation();
      }

      var frameDirection = this.GetFlowDirectionForFrame(frame);
      var flow           = this.GetOrCreateFlow(frameDirection);
      flow.AddPdu(new L7Pdu(frame, frameDirection));
      return newL7Conversation;
    }

    private L7Conversation CloseConversation()
    {
      L7Conversation newL7Conversation = null;
      if (this._upFlow != null || this._downFlow != null)
      {
        newL7Conversation = this.CreateL7Conversation(this._upFlow, this._downFlow);
        this.ResetFlows();
      }

      return newL7Conversation;
    }

    private Int64? FlowsLastSeen()
    {
      var lastSeen = Math.Max(this._upFlow?.LastSeenTicks ?? 0, this._downFlow?.LastSeenTicks ?? 0);
      if (lastSeen == 0)
      {
        return null;
      }

      return lastSeen;
    }

    private L7Flow GetOrCreateFlow(FlowDirection frameDirection)
    {
      switch (frameDirection)
      {
        case FlowDirection.Up:
          return GetOrCreateFlow(ref this._upFlow);
        case FlowDirection.Down:
          return GetOrCreateFlow(ref this._downFlow);
        default:
          throw new ArgumentOutOfRangeException(nameof(frameDirection), frameDirection, null);
      }
    }

    private static L7Flow GetOrCreateFlow(ref L7Flow flow) => flow ?? (flow = new L7Flow());

    private void ResetFlows()
    {
      this._upFlow   = null;
      this._downFlow = null;
    }
  }
}
