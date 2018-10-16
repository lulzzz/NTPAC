using System;
using PacketDotNet;

namespace NTPAC.ConversationTracking.Helpers
{
  public class Ipv4Helpers
  {
    public static Boolean Ipv4PacketIsFragmented(IPv4Packet ipv4Packet)
    {
      var fragmentOffset = ipv4Packet.FragmentOffset;
      //var moreFragments  = (ipv4Packet.FragmentFlags & 0b100) != 0;
      var moreFragments = (ipv4Packet.FragmentFlags & 0b001) != 0;
      return fragmentOffset != 0 || moreFragments;
    }
  }
}
