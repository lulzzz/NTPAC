using System;
using System.Collections.Generic;
using System.Net;
using Akka.Actor;
using Akka.Event;
using NTPAC.ConversatinTracking.Interfaces;
using NTPAC.ConversatinTracking.Interfaces.Models;
using NTPAC.Messages.L4ConversationTracking;
using NTPAC.Reassembling;
using NTPAC.Reassembling.Exceptions;

namespace NTPAC.ConversationTracking.Actors
{
  public class L4ConversationTrackingActor : ReceiveActor
  {
    private readonly IActorRef _contractor;
    private readonly IL4ConversationKey _l4Key;
    private readonly IActorRef _l7ConversationStorageActor;
    private readonly L7ConversationTrackerBase _l7ConversationTracker;
    private readonly ILoggingAdapter _logger = Context.GetLogger();

    public L4ConversationTrackingActor(IL4ConversationKey l4Key,
                                       IActorRef contractor,
                                       IPEndPoint sourceEndPoint,
                                       IPEndPoint destinationEndPoint,
                                       IActorRef l7ConversationStorageActor)
    {
      this._l4Key                      = l4Key;
      this._contractor                 = contractor;
      this._l7ConversationStorageActor = l7ConversationStorageActor;
      this._l7ConversationTracker =
        L7ConversationTrackerFactory.Create(sourceEndPoint, destinationEndPoint, l4Key.GetProtocolType);
      
      this.Become(this.AnalysisBehavior);
    }

    public static Props Props(IL4ConversationKey l4Key,
                              IActorRef contractor,
                              IPEndPoint sourceEndPoint,
                              IPEndPoint destinationEndPoint,
                              IActorRef l7ConversationStorageActor) =>
      Akka.Actor.Props.Create<L4ConversationTrackingActor>(l4Key, contractor, sourceEndPoint, destinationEndPoint,
                                              l7ConversationStorageActor);

    private void AnalysisBehavior()
    {
      this.Receive<Frame>(frame => this.OnFrame(frame));
      this.Receive<L4ConversationTrackingComplete>(complete => this.OnL4ConversationTrackingComplete(complete));
    }

    private void HandleL7Conversation(L7Conversation l7Conversation)
    {
      if (l7Conversation == null)
      {
        return;
      }

      this._logger.Debug($"L4C HandleNewL7Conversation: {l7Conversation}");

      try
      {
        this._logger.Debug($"L4C reassembled L7 conversation: {l7Conversation}");
        this._l7ConversationStorageActor.Tell(l7Conversation);
      }
      catch (Exception e)
      {
        this._logger.Error(
          e,
          $"L7Conversation delivery to L7ConversationStorage failed for conversation {l7Conversation} caused exception: {e.Message}");
        Console.WriteLine(e);
      }
    }

    private void HandleL7Conversations(IEnumerable<L7Conversation> l7Conversations)
    {
      if (l7Conversations == null)
      {
        return;
      }

      foreach (var l7Conversation in l7Conversations)
      {
        this.HandleL7Conversation(l7Conversation);
      }
    }

    private void OnFrame(Frame frame)
    {
      //this._logger.Debug("L4C OnFrame");
      
      L7Conversation l7Conversation;
      try
      {
        l7Conversation = this._l7ConversationTracker.ProcessFrame(frame);
      }
      catch (ReassemblingException e)
      {
        this._logger.Error(e, $"Frame {e.Frame} caused exception: {e.Message}");
        throw;
      }

      this.HandleL7Conversation(l7Conversation);
    }

    private void OnL4ConversationTrackingComplete(L4ConversationTrackingComplete _)
    {    
      this._logger.Debug("L4C OnFinalizeProcessing");

      IEnumerable<L7Conversation> l7Conversations;
      try
      {
        l7Conversations = this._l7ConversationTracker.Complete();
      }
      catch (ReassemblingException e)
      {
        this._logger.Error(e, $"Frame {e.Frame} caused exception: {e.Message}");
        throw;
      }

      this.HandleL7Conversations(l7Conversations);

      this._contractor.Tell(new L4ConversationTrackingCompleted(this._l4Key));
      Context.Stop(this.Self);
    }
  }
}
