using System.Net;
using NTPAC.Reassembling.Exceptions;
using NTPAC.Reassembling.TCP;
using NTPAC.Reassembling.UDP;
using PacketDotNet;

namespace NTPAC.Reassembling
{
  public static class L7ConversationTrackerFactory
  {
    public static L7ConversationTrackerBase Create(IPEndPoint sourceEndPoint,
                                                   IPEndPoint destinationEndPoint,
                                                   IPProtocolType protocolType)
    {
      switch (protocolType)
      {
        case IPProtocolType.TCP: return new TcpConversationTracker(sourceEndPoint, destinationEndPoint);
        case IPProtocolType.UDP: return new UdpConversationTracker(sourceEndPoint, destinationEndPoint);
        default:
          // TODO set frame for exception
          throw new ReassemblingException(null, $"Unsupported IP protocol: {protocolType}");
      }
    }
  }
}
