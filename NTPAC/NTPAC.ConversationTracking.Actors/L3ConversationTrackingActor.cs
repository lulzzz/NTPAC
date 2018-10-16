using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using NTPAC.AkkaSupport.Collections;
using NTPAC.Common.Extensions;
using NTPAC.ConversatinTracking.Interfaces;
using NTPAC.ConversatinTracking.Interfaces.Models;
using NTPAC.Messages.L3ConversationTracking;
using NTPAC.Messages.L4ConversationTracking;

namespace NTPAC.ConversationTracking.Actors
{
  public class L3ConversationTrackingActor : ReceiveActor
  {
    private static readonly Int64 L3ConversationInactivityTimeoutTicks = TimeSpan.FromMinutes(15).Ticks;
    private readonly IActorRef _contractor;
    private readonly IL3ConversationKey _l3Key;

    private readonly Dictionary<IL4ConversationKey, IActorRef> _l4Conversations = new Dictionary<IL4ConversationKey, IActorRef>();

    private readonly AgingActorBuffer _l4ConversationsAgingBuffer = new AgingActorBuffer(L3ConversationInactivityTimeoutTicks);
    private readonly IL4ConversationTrackingActorFactory _l4ConversationTrackingActorFactory;
    private readonly IActorRef _l7ConversationStorageActor;

    private readonly ILoggingAdapter _logger = Context.GetLogger();

    public L3ConversationTrackingActor(IL3ConversationKey l3Key,
                                       IActorRef contractor,
                                       IActorRef l7ConversationStorageActor,
                                       IL4ConversationTrackingActorFactory l4ConversationTrackingActorFactory)
    {
      this._l3Key                              = l3Key;
      this._contractor                         = contractor;
      this._l7ConversationStorageActor         = l7ConversationStorageActor;
      this._l4ConversationTrackingActorFactory = l4ConversationTrackingActorFactory;

      this.Become(this.ProcessingBehavior);
    }

    public static Props Props(IL3ConversationKey l3Key,
                              IActorRef contractor,
                              IActorRef l7ConversationStorageActor,
                              IL4ConversationTrackingActorFactory l4ConversationTrackingActorFactory) =>
      Akka.Actor.Props.Create<L3ConversationTrackingActor>(l3Key, contractor, l7ConversationStorageActor,
                                              l4ConversationTrackingActorFactory);


    private void OnFrame(Frame frame)
    {
      //this._logger.Debug("L3C OnProcessPacket");

      var l4Key = frame.L4ConversationKey;
      if (!this._l4Conversations.TryGetValue(l4Key, out var l4ConversationActor))
      {
        l4ConversationActor = this._l4ConversationTrackingActorFactory.Create(Context, l4Key, this.Self, frame.SourceEndPoint,
                                                                              frame.DestinationEndPoint,
                                                                              this._l7ConversationStorageActor,
                                                                              frame.TimestampTicks);
        this._l4Conversations.Add(l4Key, l4ConversationActor);
      }

      l4ConversationActor.Forward(frame);

      this.UpdateAndKillInactiveChildren(l4ConversationActor, frame.TimestampTicks);
    }

    private void OnL3ConversationTrackingComplete(L3ConversationTrackingComplete _)
    {
      this._logger.Debug("L3Conversation OnFinalizeProcessing");
      foreach (var child in this._l4Conversations.Values)
      {
        child.Tell(L4ConversationTrackingComplete.Instance);
      }
    }

    private void OnL4ConversationTrackingCompleted(L4ConversationTrackingCompleted completed)
    {
      this._l4Conversations.Remove(completed.L4ConversationKey);

      if (!this._l4Conversations.Any())
      {
        this._contractor.Tell(new L3ConversationTrackingCompleted(this._l3Key));
        Context.Stop(this.Self);
      }
    }

    private void ProcessingBehavior()
    {
      this.Receive<Frame>(frame => this.OnFrame(frame));
      this.Receive<L3ConversationTrackingComplete>(complete => this.OnL3ConversationTrackingComplete(complete));
      this.Receive<L4ConversationTrackingCompleted>(completed => this.OnL4ConversationTrackingCompleted(completed));
    }

    private void UpdateAndKillInactiveChildren(IActorRef l4ConversationActor, Int64 frameTimestampTicks)
    {
      this._l4ConversationsAgingBuffer.Update(l4ConversationActor, frameTimestampTicks);
      if (this._l4ConversationsAgingBuffer.HaveInactiveActors())
      {
        foreach (var inactiveL4ConversationActor in this._l4ConversationsAgingBuffer.GetAndRemoveInactiveActors())
        {
          this._logger.Info($"Sending inactivity Fin to {inactiveL4ConversationActor}");
          inactiveL4ConversationActor.Tell(L4ConversationTrackingComplete.Instance);
          this._l4Conversations.RemoveSingleReferenceValue(inactiveL4ConversationActor);
        }
      }
    }
  }
}
