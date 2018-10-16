using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NTPAC.ConversatinTracking.Interfaces.Models
{
  public class L7Pdu
  {
    private readonly List<Frame> _frames;
    private Byte[] _payloadBuffer;

    public L7Pdu(Frame frame, FlowDirection direction)
    {
      this.Direction  = direction;
      this._frames    = new List<Frame>(1) {frame};
      this.PayloadLen = frame.L7PayloadLength;
    }

    public FlowDirection Direction { get; set; }

    public Int64 FirstSeenTicks => this._frames.First().TimestampTicks;

    public IEnumerable<Frame> Frames => this._frames;
    public Int64 LastSeenTicks => this._frames.Last().TimestampTicks;

    public Byte[] Payload
    {
      get
      {
        if (this._payloadBuffer == null)
        {
          this.RebuildPayloadDataBuffer();
        }

        return this._payloadBuffer;
      }
    }

    public Int32 PayloadLen { get; set; }

    public void AddFrame(Frame frame)
    {
      this._frames.Add(frame);
      this.PayloadLen += frame.L7PayloadLength;
      // Invalidate payload data buffer
      this._payloadBuffer = null;
    }

    private void RebuildPayloadDataBuffer()
    {
      var ms = new MemoryStream(new Byte[this.PayloadLen], 0, this.PayloadLen, true, true);
      foreach (var frame in this._frames)
      {
        var frameL7Payload = frame.L7Payload;
        ms.Write(frameL7Payload, 0, frameL7Payload.Length);
      }

      this._payloadBuffer = ms.GetBuffer();
    }

    public override string ToString() => $"{new DateTime(this.FirstSeenTicks).ToLocalTime()} > {this.Direction.ToString()} > {this.PayloadLen} B";
  }
}
