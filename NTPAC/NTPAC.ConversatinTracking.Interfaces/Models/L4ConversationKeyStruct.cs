using System;
using PacketDotNet;

namespace NTPAC.ConversatinTracking.Interfaces.Models
{
  public struct L4ConversationKeyStruct
  {
    public UInt32 Key { get; }
    public IPProtocolType Protocol { get; }

    public L4ConversationKeyStruct(UInt16 port1, UInt16 port2, IPProtocolType protocol)
    {
      this.Key      = port1 > port2 ? (UInt32) ((port1 << 16) + port2) : (UInt32) ((port2 << 16) + port1);
      this.Protocol = protocol;
    }

    public static Boolean operator ==(L4ConversationKeyStruct key1, L4ConversationKeyStruct key2) => Equals(key1, key2);

    public static Boolean operator !=(L4ConversationKeyStruct key1, L4ConversationKeyStruct key2) => !Equals(key1, key2);

    public Boolean Equals(L4ConversationKeyStruct other) => this.Key == other.Key && this.Protocol == other.Protocol;

    public override Boolean Equals(Object obj)
    {
      if (obj is null)
      {
        return false;
      }

      return obj is L4ConversationKeyStruct @struct && this.Equals(@struct);
    }

    public override Int32 GetHashCode()
    {
      unchecked
      {
        return ((Int32) this.Key * 397) ^ (Int32) this.Protocol;
      }
    }

    public override String ToString()
    {
      var port1 = this.Key         & 0xFFFF;
      var port2 = (this.Key >> 16) & 0xFFFF;
      return $"{port1}:{port2}{this.Protocol}";
    }
  }
}
