using System;
using System.Collections.Generic;
using System.Net;
using NTPAC.ConversatinTracking.Interfaces.Models;
using PacketDotNet;

namespace NTPAC.Reassembling
{
  public abstract class L7ConversationTrackerBase
  {
    protected L7ConversationTrackerBase(IPEndPoint sourceEndPoint, IPEndPoint destinationEndPoint)
    {
      this.SourceEndPoint      = sourceEndPoint;
      this.DestinationEndPoint = destinationEndPoint;
    }

    public IPEndPoint DestinationEndPoint { get; }
    public abstract IPProtocolType ProtocolType { get; }
    public IPEndPoint SourceEndPoint { get; }

    public abstract IEnumerable<L7Conversation> Complete();
    public abstract L7Conversation ProcessFrame(Frame frame);

    protected L7Conversation CreateL7Conversation(L7Flow upFlow, L7Flow downFlow) =>
      new L7Conversation(this.SourceEndPoint, this.DestinationEndPoint, this.ProtocolType, upFlow, downFlow);

    protected FlowDirection GetFlowDirectionForFrame(Frame frame)
    {
      if (frame.IpProtocol != this.ProtocolType)
      {
        return FlowDirection.None;
      }

      if (this.IsUpFlowPacket(frame))
      {
        return FlowDirection.Up;
      }

      if (this.IsDownFlowPacket(frame))
      {
        return FlowDirection.Down;
      }

      return FlowDirection.None;
    }

    private Boolean IsDownFlowPacket(Frame frame) =>
      frame.SourceAddress.Equals(this.DestinationEndPoint.Address) &&
      frame.SourcePort == this.DestinationEndPoint.Port            &&
      frame.DestinationAddress.Equals(this.SourceEndPoint.Address) &&
      frame.DestinationPort == this.SourceEndPoint.Port;

    private Boolean IsUpFlowPacket(Frame frame) =>
      frame.SourceAddress.Equals(this.SourceEndPoint.Address)           &&
      frame.SourcePort == this.SourceEndPoint.Port                      &&
      frame.DestinationAddress.Equals(this.DestinationEndPoint.Address) &&
      frame.DestinationPort == this.DestinationEndPoint.Port;
  }
}
