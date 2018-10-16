using System;
using System.Collections.Generic;
using System.Linq;

namespace NTPAC.ConversatinTracking.Interfaces.Models
{
  public class L7Flow
  {
    private readonly List<Frame> _nonDataFrames = new List<Frame>();
    private readonly List<L7Pdu> _pdus = new List<L7Pdu>();

    public L7Flow() { }
    public L7Flow(UInt32 identifier) => this.FlowIdentifier = identifier;
    public Int64? FirstSeenTicks { get; private set; }

    public UInt32? FlowIdentifier { get; }
    public IEnumerable<L7Pdu> L7Pdus => this._pdus;
    public Int64? LastSeenTicks { get; private set; }
    public IEnumerable<Frame> NonDataFrames => this._nonDataFrames;

    // Whether this up/down flow was paired with its down/up flow counterpart or was classified as a bidirectional flow
    public Boolean Paired { get; set; }

    public void AddNonDataFrame(Frame frame)
    {
      if (this.FirstSeenTicks == null || this.FirstSeenTicks > frame.TimestampTicks)
      {
        this.FirstSeenTicks = frame.TimestampTicks;
      }

      if (this.LastSeenTicks == null || this.LastSeenTicks < frame.TimestampTicks)
      {
        this.LastSeenTicks = frame.TimestampTicks;
      }

      this._nonDataFrames.Add(frame);
    }

    public void AddPdu(L7Pdu pdu)
    {
      if (this.FirstSeenTicks == null || this.FirstSeenTicks > pdu.FirstSeenTicks)
      {
        this.FirstSeenTicks = pdu.FirstSeenTicks;
      }

      if (this.LastSeenTicks == null || this.LastSeenTicks < pdu.LastSeenTicks)
      {
        this.LastSeenTicks = pdu.LastSeenTicks;
      }

      this._pdus.Add(pdu);
    }

    public Boolean Any() => this._pdus.Any();

    public void SetPaired() { this.Paired = true; }
  }
}
