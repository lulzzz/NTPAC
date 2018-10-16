using System;
using NTPAC.ConversatinTracking.Interfaces.Models;
using NTPAC.ConversationTracking.Helpers;
using PacketDotNet;

namespace NTPAC.ConversationTracking.Factories
{
  public static class FrameFactory
  {
    public static Frame CreateFromIpPacket(IPv4Packet ipPacket, Int64 timestamp)
    {
      var frame = new Frame {TimestampTicks = timestamp};
      frame.IsValid = DissectSourceIpPacket(frame, ipPacket);
      return frame;
    }

    public static Frame CreateFromPacket(Packet packet, Int64 timestamp)
    {
      var frame = new Frame {TimestampTicks = timestamp};
      frame.IsValid = DissectSourcePacket(frame, packet);
      return frame;
    }

    private static Boolean DissectSourceIpPacket(Frame frame, IPPacket ipPacket)
    {
      frame.SourceAddress      = ipPacket.SourceAddress;
      frame.DestinationAddress = ipPacket.DestinationAddress;
      frame.IpProtocol         = ipPacket.Protocol;

      if (TryDissectFragmentedIpPacket(frame, ipPacket))
      {
        return true;
      }

      if (TryDissectNonTransportPacket(frame, ipPacket))
      {
        return true;
      }

      if (TryDissectTransportPacket(frame, ipPacket))
      {
        return true;
      }

      ;

      return true;
    }

    private static Boolean DissectSourcePacket(Frame frame, Packet packet)
    {
      if (!(packet?.PayloadPacket is IPPacket ipPacket))
      {
        return false;
      }

      return DissectSourceIpPacket(frame, ipPacket);
    }

    private static void DissectTcpPacket(Frame frame, TcpPacket tcpPacket)
    {
      frame.TcpFFin = tcpPacket.Fin;
      frame.TcpFSyn = tcpPacket.Syn;
      frame.TcpFRst = tcpPacket.Rst;
      frame.TcpFPsh = tcpPacket.Psh;
      frame.TcpFAck = tcpPacket.Ack;
      frame.TcpFUrg = tcpPacket.Urg;
      frame.TcpFEcn = tcpPacket.ECN;
      frame.TcpFCwr = tcpPacket.CWR;

      frame.TcpSequenceNumber       = tcpPacket.SequenceNumber;
      frame.TcpAcknowledgmentNumber = tcpPacket.AcknowledgmentNumber;
      frame.TcpChecksum             = tcpPacket.Checksum;
      frame.TcpChecksumValid        = tcpPacket.ValidChecksum;
    }

    private static Boolean TryDissectFragmentedIpPacket(Frame frame, IPPacket ipPacket)
    {
      if (ipPacket is IPv4Packet ipv4Packet && Ipv4Helpers.Ipv4PacketIsFragmented(ipv4Packet))
      {
        frame.IsIpv4Fragmented = true;

        frame.Key = new IPFragmentKey(ipv4Packet);

        frame.Offset        = ipv4Packet.FragmentOffset * 8;
        frame.MoreFragments = (ipv4Packet.FragmentFlags & 0b001) != 0;
        frame.L7Payload     = ipv4Packet.PayloadData ?? ipv4Packet.PayloadPacket.Bytes;
        frame.HeaderLen     = ipv4Packet.HeaderLength;
        return true;
      }

      return false;
    }

    private static Boolean TryDissectNonTransportPacket(Frame frame, IPPacket ipPacket)
    {
      if (ipPacket.PayloadPacket is TransportPacket)
      {
        return false;
      }

      frame.L7Payload = ipPacket.PayloadData;
      return false;
    }

    private static Boolean TryDissectTransportPacket(Frame frame, IPPacket ipPacket)
    {
      if (!(ipPacket.PayloadPacket is TransportPacket sourceDestinationPort))
      {
        return false;
      }

      frame.SourcePort             = sourceDestinationPort.SourcePort;
      frame.DestinationPort        = sourceDestinationPort.DestinationPort;
      frame.IsValidTransportPacket = true;

      if (sourceDestinationPort is TcpPacket tcpPacket)
      {
        DissectTcpPacket(frame, tcpPacket);
      }

      frame.L7Payload = ipPacket.PayloadPacket?.PayloadData;

      return true;
    }
  }
}
