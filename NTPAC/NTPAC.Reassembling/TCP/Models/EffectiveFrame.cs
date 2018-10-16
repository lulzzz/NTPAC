using System;
using NTPAC.Common.Extensions;
using NTPAC.ConversatinTracking.Interfaces.Models;

namespace NTPAC.Reassembling.TCP.Models
{
  internal struct EffectiveFrame
  {
    public UInt64 RelativeOffsetBegin { get; private set; }
    public UInt64 RelativeOffsetEnd { get; private set; }
    public Frame Frame { get; }

    public EffectiveFrame(Frame frame, UInt32 seqNumOverflows = 0) : this()
    {
      this.Frame               = frame;
      this.RelativeOffsetBegin = frame.TcpSequenceNumber  + UInt32.MaxValue * (UInt64) seqNumOverflows;
      this.RelativeOffsetEnd   = this.RelativeOffsetBegin + (UInt64) frame.L7PayloadLength;

      if (frame.IsKeepAlive)
      {
        // Normalize Sequence numbers for KeepAlive frames (which have Sequence number decreased by 1)
        this.RelativeOffsetBegin++;
        this.RelativeOffsetEnd++;
      }
    }

    public Boolean Equals(EffectiveFrame other) => Equals(this.Frame, other.Frame);

    public override Boolean Equals(Object obj) => obj is EffectiveFrame frame && this.Equals(frame);

    public override Int32 GetHashCode() => (this.Frame != null ? this.Frame.GetHashCode() : 0);

    public Byte[] Payload => this.Frame.L7Payload;
    public UInt64 PayloadLen => this.RelativeOffsetEnd - this.RelativeOffsetBegin;

    public Boolean PayloadsOverlapMismatch(EffectiveFrame other)
    {
      if (this.RelativeOffsetBegin <= other.RelativeOffsetEnd && other.RelativeOffsetBegin <= this.RelativeOffsetEnd)
      {
        var overlapBegin = Math.Max(this.RelativeOffsetBegin, other.RelativeOffsetBegin);
        var overlapEnd   = Math.Min(this.RelativeOffsetEnd, other.RelativeOffsetEnd);
        var overlapLen   = (Int32) (overlapEnd - overlapBegin);

        var payloadOffset      = (Int32) (overlapBegin - this.RelativeOffsetBegin);
        var otherPayloadOffset = (Int32) (overlapBegin - other.RelativeOffsetBegin);

        return !this.Payload.ContentsEqual(other.Payload, payloadOffset, otherPayloadOffset, overlapLen);
      }

      // If there is no overlap, there is no mismatch
      return false;
    }

    public void NormalizeKeepAliveSequenceNumbers()
    {
      if (!this.Frame.IsKeepAlive)
      {
        return;
      }

      this.RelativeOffsetBegin++;
      this.RelativeOffsetEnd++;
    }

    public override String ToString() => $"Seq:{this.RelativeOffsetBegin}-{this.RelativeOffsetEnd} > {this.Frame}";
  }
}
