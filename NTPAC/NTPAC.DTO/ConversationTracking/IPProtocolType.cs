namespace NTPAC.DTO.ConversationTracking
{
  public enum IPProtocolType : byte
  {
    HOPOPTS = 0,
    IP = 0,
    ICMP = 1,
    IGMP = 2,
    IPIP = 4,
    TCP = 6,
    EGP = 8,
    PUP = 12,      // 0x0C
    UDP = 17,      // 0x11
    IDP = 22,      // 0x16
    TP = 29,       // 0x1D
    IPV6 = 41,     // 0x29
    ROUTING = 43,  // 0x2B
    FRAGMENT = 44, // 0x2C
    RSVP = 46,     // 0x2E
    GRE = 47,      // 0x2F
    ESP = 50,      // 0x32
    AH = 51,       // 0x33
    ICMPV6 = 58,   // 0x3A
    NONE = 59,     // 0x3B
    DSTOPTS = 60,  // 0x3C
    OSPF = 89,     // 0x59
    MTP = 92,      // 0x5C
    ENCAP = 98,    // 0x62
    PIM = 103,     // 0x67
    COMP = 108,    // 0x6C
    MASK = 255,    // 0xFF
    RAW = 255      // 0xFF
  }
}
