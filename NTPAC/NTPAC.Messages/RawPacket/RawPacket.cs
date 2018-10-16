using System;
using System.Diagnostics.CodeAnalysis;
using MessagePack;
using NTPAC.Common.Interfaces;
using NTPAC.ConversatinTracking.Interfaces.Models;
using PacketDotNet;
using SharpPcap;

namespace NTPAC.Messages.RawPacket
{
  [MessagePackObject]
  [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
  public class RawPacket : ISingleValue<RawPacket>
  {
    [Key(0)] public Int64 DateTimeTicks;

    [IgnoreMember] public Int32 EntityId;

    [Key(1)] public LinkLayers LinkType;

    [Key(2)] public Byte[] RawPacketData;
    public RawPacket(RawCapture rawCapture) : this(rawCapture.Timeval.Date.Ticks, rawCapture.LinkLayerType, rawCapture.Data) { }

    public RawPacket(RawCapture rawCapture, L3L4ConversationKey l3L4ConversationKey, Int32 maxNumberOfShards) : this(
      rawCapture.Timeval.Date.Ticks, rawCapture.LinkLayerType, rawCapture.Data, l3L4ConversationKey, maxNumberOfShards)
    {
    }

    public RawPacket() { }

    public RawPacket(Int64 dateTimeTicks,
                     LinkLayers linkType,
                     Byte[] rawPacketData,
                     L3L4ConversationKey l3L4ConversationKey,
                     Int32 maxNumberOfShards) : this(dateTimeTicks, linkType, rawPacketData) =>
      this.EntityId = Math.Abs(l3L4ConversationKey.GetHashCode() % maxNumberOfShards);

    public RawPacket(Int64 dateTimeTicks, LinkLayers linkType, Byte[] rawPacketData)
    {
      this.DateTimeTicks = dateTimeTicks;
      this.LinkType      = linkType;
      this.RawPacketData = rawPacketData;
    }

    public RawCapture RecreateOriginalRawCapture()
    {
      var secs  = this.DateTimeTicks                                    / TimeSpan.TicksPerSecond;
      var usecs = (this.DateTimeTicks - secs * TimeSpan.TicksPerSecond) / (TimeSpan.TicksPerMillisecond / 1000);
      return new RawCapture(this.LinkType, new PosixTimeval((UInt64) secs, (UInt64) usecs), this.RawPacketData);
    }

    public override String ToString() =>
      $"{nameof(this.DateTimeTicks)}: {new DateTime(this.DateTimeTicks)}, {nameof(this.EntityId)}: {this.EntityId}, {nameof(this.LinkType)}: {this.LinkType}";
  }
}
