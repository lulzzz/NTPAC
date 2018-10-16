using System;
using System.Collections.Generic;
using Akka.Streams;
using Akka.Streams.Stage;
using Akka.Streams.Supervision;
using NTPAC.Common.Interfaces;

namespace NTPAC.AkkaSupport.GraphStages
{
  public class MaybeSelectMultipleValues<T> : GraphStage<FlowShape<IMaybeMultipleValues<T>, T>> where T : class
  {
    private readonly Inlet<IMaybeMultipleValues<T>> _in = new Inlet<IMaybeMultipleValues<T>>("MaybeSelectMultipleValues.in");
    private readonly Outlet<T> _out = new Outlet<T>("MaybeSelectMultipleValues.out");

    public MaybeSelectMultipleValues() => this.Shape = new FlowShape<IMaybeMultipleValues<T>, T>(this._in, this._out);

    public override FlowShape<IMaybeMultipleValues<T>, T> Shape { get; }

    public override String ToString() => "MaybeSelectMultipleValues";

    protected override GraphStageLogic CreateLogic(Attributes inheritedAttributes) => new Logic(this, inheritedAttributes);

    private sealed class Logic : InAndOutGraphStageLogic
    {
      private readonly Decider _decider;
      private readonly MaybeSelectMultipleValues<T> _stage;

      private IMaybeMultipleValues<T> _current;
      private IEnumerator<T> _currentEnumerator;
      private Boolean _currentEnumeratorHasValue;

      public Logic(MaybeSelectMultipleValues<T> stage, Attributes inheritedAttributes) : base(stage.Shape)
      {
        this._stage = stage;
        this._decider = inheritedAttributes.GetAttribute(new ActorAttributes.SupervisionStrategy(Deciders.StoppingDecider))
                                           .Decider;

        this.SetHandler(stage._in, stage._out, this);
      }

      private Boolean EnumeratorHasValue => this._currentEnumerator != null && this._currentEnumeratorHasValue;

      public override void OnPull() { this.PushPull(); }

      public override void OnPush()
      {
        try
        {
          this._current = this.Grab(this._stage._in);
          if (this._current is IMultipleValues<T> mv)
          {
            this._currentEnumerator = mv.Values.GetEnumerator();
            this.MoveCurrentEnumerator();
          }

          this.PushPull();
        }
        catch (Exception ex)
        {
          var directive = this._decider(ex);
          switch (directive)
          {
            case Directive.Stop:
              this.FailStage(ex);
              break;
            case Directive.Resume:
              if (!this.HasBeenPulled(this._stage._in)) this.Pull(this._stage._in);
              break;
            case Directive.Restart:
              this.RestartState();
              if (!this.HasBeenPulled(this._stage._in)) this.Pull(this._stage._in);
              break;
            default:
              throw new ArgumentOutOfRangeException();
          }
        }
      }

      private void MoveCurrentEnumerator() { this._currentEnumeratorHasValue = this._currentEnumerator.MoveNext(); }


      private void PushPull()
      {
        if (this._current is ISingleValue<T> singleValue)
        {
          if (singleValue is T value)
          {
            this.Push(this._stage._out, value);
            this.RestartState();
          }
          else
          {
            throw new ArgumentException("ISingleValue must have its type parameter set to given conrete class");
          }
        }
        else if (this._current is IMultipleValues<T> && this.EnumeratorHasValue)
        {
          this.Push(this._stage._out, this._currentEnumerator.Current);
          this.MoveCurrentEnumerator();
          if (!this.EnumeratorHasValue && this.IsClosed(this._stage._in))
          {
            this.CompleteStage();
          }
        }
        else if (!this.IsClosed(this._stage._in))
        {
          this.Pull(this._stage._in);
        }
        else
        {
          this.CompleteStage();
        }
      }

      private void RestartState()
      {
        this._current           = null;
        this._currentEnumerator = null;
      }
    }
  }
}
