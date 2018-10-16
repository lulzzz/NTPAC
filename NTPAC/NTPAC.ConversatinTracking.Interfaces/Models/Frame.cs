using System;
using System.Net;
using PacketDotNet;

namespace NTPAC.ConversatinTracking.Interfaces.Models
{
  public class Frame
  {
    private IPEndPoint _destinationEndPoint;

    private Boolean _isKeepAlive;
    private IL3ConversationKey _l3ConversationKey;
    private IL4ConversationKey _l4ConversationKey;
    private IPEndPoint _sourceEndPoint;
    public IPAddress DestinationAddress { get; set; }

    public IPEndPoint DestinationEndPoint =>
      this._destinationEndPoint ?? (this._destinationEndPoint = new IPEndPoint(this.DestinationAddress, this.DestinationPort));

    public UInt16 DestinationPort { get; set; }

    public String FormatedTimestamp => (new DateTime(this.TimestampTicks)).ToLocalTime().ToString("HH:mm:ss.ffffff");

    public Int32 HeaderLen { get; set; }
    public IPProtocolType IpProtocol { get; set; }
    public Boolean IsIpv4Fragmented { get; set; }

    public Boolean IsKeepAlive
    {
      get => this._isKeepAlive;
      set
      {
        if (value)
        {
          this.ClearL7Payload();
        }

        this._isKeepAlive = value;
      }
    }

    public Boolean IsRetransmission { get; set; }
    public Boolean IsValid { get; set; }
    public Boolean IsValidTransportPacket { get; set; }
    public IPFragmentKey Key { get; set; }

    public IL3ConversationKey L3ConversationKey =>
      this._l3ConversationKey ??
      (this._l3ConversationKey = new L3ConversationKeyClass(this.SourceAddress, this.DestinationAddress));

    public IL4ConversationKey L4ConversationKey =>
      this._l4ConversationKey ??
      (this._l4ConversationKey = new L4ConversationKeyClass(this.SourcePort, this.DestinationPort, this.IpProtocol));

    public Byte[] L7Payload { get; set; }
    public Int32 L7PayloadLength => this.L7Payload?.Length ?? 0;
    public Boolean MoreFragments { get; set; }
    public Int32 Offset { get; set; }
    public IPAddress SourceAddress { get; set; }

    public IPEndPoint SourceEndPoint =>
      this._sourceEndPoint ?? (this._sourceEndPoint = new IPEndPoint(this.SourceAddress, this.SourcePort));

    public UInt16 SourcePort { get; set; }
    public UInt32 TcpAcknowledgmentNumber { get; set; }
    public UInt16 TcpChecksum { get; set; }

    public Boolean TcpChecksumValid { get; set; }

    //TODO should be implemented as FLAGS enum
    public Boolean TcpFAck { get; set; }
    public Boolean TcpFCwr { get; set; }
    public Boolean TcpFEcn { get; set; }
    public Boolean TcpFFin { get; set; }
    public Boolean TcpFPsh { get; set; }
    public Boolean TcpFRst { get; set; }
    public Boolean TcpFSyn { get; set; }
    public Boolean TcpFUrg { get; set; }
    public UInt32 TcpSequenceNumber { get; set; }
    public Int64 TimestampTicks { get; set; }

    public override String ToString() =>
      $"{this?.FormatedTimestamp ?? ""} > {this?.SourceEndPoint}-{this?.DestinationEndPoint} > {this?.L7PayloadLength}";

    private void ClearL7Payload() { this.L7Payload = new Byte[0]; }
  }
}
