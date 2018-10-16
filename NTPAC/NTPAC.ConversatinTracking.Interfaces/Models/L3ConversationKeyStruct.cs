using System;
using System.Net;
using PacketDotNet;

namespace NTPAC.ConversatinTracking.Interfaces.Models
{
  public struct L3ConversationKeyStruct
  {
    public IPAddress Address1 { get; }
    public IPAddress Address2 { get; }

    public L3ConversationKeyStruct(IPAddress address1, IPAddress address2)
    {
      var key = GetKey(address1, address2);
      this.Address1 = key.address1;
      this.Address2 = key.address2;
    }

    public L3ConversationKeyStruct(IPPacket ipPacket) : this(ipPacket.SourceAddress, ipPacket.DestinationAddress) { }

    public static Boolean Equals(L3ConversationKeyStruct key1, L3ConversationKeyStruct key2) =>
      key1.Address1.Equals(key2.Address1) && key1.Address2.Equals(key2.Address2);

    public override Boolean Equals(Object obj)
    {
      if (obj is null)
      {
        return false;
      }

      return obj.GetType() == this.GetType() && this.Equals((L3ConversationKeyStruct) obj);
    }

    public override Int32 GetHashCode()
    {
      unchecked
      {
        return ((this.Address1 != null ? this.Address1.GetHashCode() : 0) * 397) ^
               (this.Address2 != null ? this.Address2.GetHashCode() : 0);
      }
    }

    public static Boolean operator ==(L3ConversationKeyStruct key1, L3ConversationKeyStruct key2) => Equals(key1, key2);
    public static Boolean operator !=(L3ConversationKeyStruct key1, L3ConversationKeyStruct key2) => !Equals(key1, key2);
    public override String ToString() => $"{this.Address1}:{this.Address2}";

    public Boolean Equals(L3ConversationKeyStruct other) =>
      Equals(this.Address1, other.Address1) && Equals(this.Address2, other.Address2);

    private static (IPAddress address1, IPAddress address2) GetKey(IPAddress address1, IPAddress address2) =>
      address1.GetHashCode() > address2.GetHashCode() ? (address1, address2) : (address2, address1);
  }
}
