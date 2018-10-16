using System;
using System.Net;
using PacketDotNet;

namespace NTPAC.ConversatinTracking.Interfaces.Models
{
  public class IPFragmentKey
  {
    public IPFragmentKey(IPv4Packet ipv4Packet)
    {
      this.SourceAddress      = ipv4Packet.SourceAddress;
      this.DestinationAddress = ipv4Packet.DestinationAddress;
      this.Protocol           = ipv4Packet.Protocol;
      this.IPIdentification   = ipv4Packet.Id;
    }

    public IPAddress DestinationAddress { get; }
    public Int32 IPIdentification { get; }
    public IPProtocolType Protocol { get; }
    public IPAddress SourceAddress { get; }

    public override Boolean Equals(Object obj)
    {
      if (obj is null)
      {
        return false;
      }

      if (ReferenceEquals(this, obj))
      {
        return true;
      }

      if (obj.GetType() != this.GetType())
      {
        return false;
      }

      return this.Equals((IPFragmentKey) obj);
    }

    public override Int32 GetHashCode()
    {
      unchecked
      {
        var hashCode = (this.SourceAddress != null ? this.SourceAddress.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (this.DestinationAddress != null ? this.DestinationAddress.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (Int32) this.Protocol;
        hashCode = (hashCode * 397) ^ this.IPIdentification;
        return hashCode;
      }
    }

    protected Boolean Equals(IPFragmentKey other) =>
      Equals(this.SourceAddress, other.SourceAddress)           &&
      Equals(this.DestinationAddress, other.DestinationAddress) &&
      this.Protocol         == other.Protocol                   &&
      this.IPIdentification == other.IPIdentification;
  }
}
