using System;
using System.Collections.Generic;
using Akka.Streams;
using Akka.Streams.Stage;
using NTPAC.Messages.RawPacket;

namespace NTPAC.AkkaSupport.GraphStages
{
  public class GroupedByCountAndRawCaptureSizeWithin : GraphStage<FlowShape<RawPacket, IEnumerable<RawPacket>>>
  {
    private readonly Int32 _count;
    private readonly Inlet<RawPacket> _in = new Inlet<RawPacket>("GroupedByCountAndRawCaptureSizeWithin.in");

    private readonly Outlet<IEnumerable<RawPacket>> _out =
      new Outlet<IEnumerable<RawPacket>>("GroupedByCountAndRawCaptureSizeWithin.out");

    private readonly Int32 _rawCapturesSizeLimit;
    private readonly TimeSpan _timeout;

    public GroupedByCountAndRawCaptureSizeWithin(Int32 count, Int32 rawCapturesSizeLimit, TimeSpan timeout)
    {
      this.Shape                 = new FlowShape<RawPacket, IEnumerable<RawPacket>>(this._in, this._out);
      this._count                = count;
      this._rawCapturesSizeLimit = rawCapturesSizeLimit;
      this._timeout              = timeout;
    }

    public override FlowShape<RawPacket, IEnumerable<RawPacket>> Shape { get; }

    public override String ToString() => "GroupedByCountAndRawCaptureSizeWithin";

    protected override GraphStageLogic CreateLogic(Attributes inheritedAttributes) => new Logic(this);


    private sealed class Logic : TimerGraphStageLogic, IInHandler, IOutHandler
    {
      private const String GroupedWithinTimer = "GroupedWithinTimer";

      private readonly GroupedByCountAndRawCaptureSizeWithin _stage;
      private List<RawPacket> _buffer;
      private Int32 _elements;
      private Boolean _finished;

      // True if:
      // - buf is nonEmpty
      //       AND
      // - timer fired OR group is full
      private Boolean _groupClosed;
      private Boolean _groupEmitted = true;
      private Int32 _rawCapturesSizeSum;

      public Logic(GroupedByCountAndRawCaptureSizeWithin stage) : base(stage.Shape)
      {
        this._stage  = stage;
        this._buffer = new List<RawPacket>(this._stage._rawCapturesSizeLimit);

        this.SetHandler(this._stage._in, this);
        this.SetHandler(this._stage._out, this);
      }

      public override void PreStart()
      {
        this.ScheduleRepeatedly(GroupedWithinTimer, this._stage._timeout);
        this.Pull(this._stage._in);
      }

      protected override void OnTimer(Object timerKey)
      {
        if (this._elements > 0) this.CloseGroup();
      }

      private void CloseGroup()
      {
        this._groupClosed = true;
        if (this.IsAvailable(this._stage._out)) this.EmitGroup();
      }

      private void EmitGroup()
      {
        this._groupEmitted = true;
        this.Push(this._stage._out, this._buffer);
        this._buffer = new List<RawPacket>(this._stage._count);
        if (!this._finished)
          this.StartNewGroup();
        else
          this.CompleteStage();
      }

      private void NextElement(RawPacket element)
      {
        this._groupEmitted = false;
        this._buffer.Add(element);
        this._elements++;
        this._rawCapturesSizeSum += element.RawPacketData.Length;
        if (this._elements == this._stage._count || this._rawCapturesSizeSum >= this._stage._rawCapturesSizeLimit)
        {
          this.ScheduleRepeatedly(GroupedWithinTimer, this._stage._timeout);
          this.CloseGroup();
        }
        else
        {
          this.Pull(this._stage._in);
        }
      }

      private void StartNewGroup()
      {
        this._elements           = 0;
        this._rawCapturesSizeSum = 0;
        this._groupClosed        = false;
        if (this.IsAvailable(this._stage._in))
          this.NextElement(this.Grab(this._stage._in));
        else if (!this.HasBeenPulled(this._stage._in)) this.Pull(this._stage._in);
      }

      public void OnPush()
      {
        if (!this._groupClosed) this.NextElement(this.Grab(this._stage._in)); // otherwise keep the element for next round
      }

      public void OnUpstreamFailure(Exception e) => this.FailStage(e);

      public void OnUpstreamFinish()
      {
        this._finished = true;
        if (this._groupEmitted)
          this.CompleteStage();
        else
          this.CloseGroup();
      }

      public void OnDownstreamFinish() => this.CompleteStage();

      public void OnPull()
      {
        if (this._groupClosed) this.EmitGroup();
      }
    }
  }
}
