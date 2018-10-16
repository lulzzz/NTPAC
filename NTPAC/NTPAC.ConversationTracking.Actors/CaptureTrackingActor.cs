using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using NTPAC.AkkaSupport;
using NTPAC.ConversatinTracking.Interfaces;
using NTPAC.ConversatinTracking.Interfaces.Models;
using NTPAC.ConversationTracking.Factories;
using NTPAC.Messages.CaptureTracking;
using NTPAC.Messages.L3ConversationTracking;
using NTPAC.Messages.L7ConversationStorage;
using NTPAC.Reassembling.IP;

namespace NTPAC.ConversationTracking.Actors
{
  public class CaptureTrackingActor : ReceiveActor
  {
    private readonly CaptureInfo _captureInfo;
    private readonly IActorRef _contractor;
    private readonly Ipv4DefragmentationEngine _ipv4DefragmentationEngine;

    private readonly Dictionary<IL3ConversationKey, IActorRef> _l3Conversations = new Dictionary<IL3ConversationKey, IActorRef>();
    private readonly IL3ConversationTrackingActorFactory _l3ConversationTrackingActorFactory;
    private readonly IActorRef _l7ConversationStorageActor;
    private readonly IL7ConversationStorageActorFactory _l7ConversationStorageActorFactory;

    private readonly ILoggingAdapter _logger = Context.GetLogger();

    private CaptureTrackingComplete _completeRequest;

    public CaptureTrackingActor(CaptureInfo captureInfo,
                                IActorRef contractor,
                                IL3ConversationTrackingActorFactory l3ConversationTrackingActorFactory,
                                IL7ConversationStorageActorFactory l7ConversationStorageActorFactory)
    {
      this._captureInfo                        = captureInfo;
      this._contractor                         = contractor;
      this._l3ConversationTrackingActorFactory = l3ConversationTrackingActorFactory;
      this._l7ConversationStorageActorFactory  = l7ConversationStorageActorFactory;
      this._ipv4DefragmentationEngine =
        new Ipv4DefragmentationEngine(new AkkaLoggingAdapter<Ipv4DefragmentationEngine>(this._logger));

      this._logger.Info($"Started for {captureInfo.Uri.AbsoluteUri}");
      this._l7ConversationStorageActor = this.CreateL7ConversationStorageActor();

      this.Become(this.AnalysisBehavior);
    }

    public static Props Props(CaptureInfo info,
                              IActorRef contractor,
                              IL3ConversationTrackingActorFactory l3ConversationTrackingActorFactory,
                              IL7ConversationStorageActorFactory l7ConversationStorageActorFactory) =>
      Akka.Actor.Props.Create<CaptureTrackingActor>(info, contractor, l3ConversationTrackingActorFactory,
                                                             l7ConversationStorageActorFactory);

    private void AnalysisBehavior()
    {
      this.Receive<Frame>(frame => this.OnFrame(frame));
      this.Receive<CaptureTrackingComplete>(complete => this.OnCaptureTrackingComplete(complete));
      this.Receive<L3ConversationTrackingCompleted>(completed => this.OnL3ConversationTrackingCompleted(completed));
      this.Receive<L7ConversationStorageCompleted>(completed => this.OnL7ConversationStorageCompleted());
    }

    private void CompleteL3ConversationTrackers()
    {
      foreach (var l3ConversationActor in this._l3Conversations.Values)
      {
        l3ConversationActor.Tell(L3ConversationTrackingComplete.Instance);
      }
    }

    private IActorRef CreateL3ConversationActor(IL3ConversationKey l3Key)
    {
      this._logger.Debug($"Creating new L3C actor: {l3Key}");
      var l3ConversationActor =
        this._l3ConversationTrackingActorFactory.Create(Context, l3Key, this.Self, this._l7ConversationStorageActor);
      this._l3Conversations.Add(l3Key, l3ConversationActor);
      return l3ConversationActor;
    }

    private IActorRef CreateL7ConversationStorageActor()
    {
      var l7ConversationStorageActor = this._l7ConversationStorageActorFactory.Create(Context, this._captureInfo, this.Self);
      Context.Watch(l7ConversationStorageActor);

      return l7ConversationStorageActor;
    }

    private void OnCaptureTrackingComplete(CaptureTrackingComplete complete)
    {
      this._logger.Debug("CaptureTrackingComplete request");
      this._completeRequest = complete;
      this.CompleteL3ConversationTrackers();
    }

    private void OnFrame(Frame frame)
    {
      if (frame.IsIpv4Fragmented)
      {
        if (!this.TryDefragmentFrame(ref frame))
        {
          return;
        }
      }

      this.TrackL3ConversationForFrame(frame);
    }

    private void OnL3ConversationTrackingCompleted(L3ConversationTrackingCompleted l3ConversationTrackingCompleted)
    {
      this._l3Conversations.Remove(l3ConversationTrackingCompleted.L3ConversationKey);
      if (!this._l3Conversations.Any())
      {
        this._l7ConversationStorageActor.Tell(L7ConversationStorageComplete.Instance);
      }
    }

    private void OnL7ConversationStorageCompleted()
    {
      if (this._completeRequest == null)
      {
        throw new Exception("Received OnL7ConversationStorageComplete without prior CaptureTrackingCompleted");
      }

      var completed = new CaptureTrackingCompleted {MessageId = this._completeRequest.MessageId};
      this._contractor.Tell(completed);
      Context.Stop(this.Self);
    }

    private void TrackL3ConversationForFrame(Frame frame)
    {
      var l3Key = frame.L3ConversationKey;
      if (!this._l3Conversations.TryGetValue(l3Key, out var l3ConversationActor))
      {
        l3ConversationActor = this.CreateL3ConversationActor(l3Key);
      }

      l3ConversationActor.Tell(frame);
    }

    private Boolean TryDefragmentFrame(ref Frame frame)
    {
      var (isDefragmentationSuccessful, firstTimeStamp, defragmentedIpv4Packet) =
        this._ipv4DefragmentationEngine.TryDefragmentFragments(frame);
      if (!isDefragmentationSuccessful)
      {
        this._logger.Debug("Packet fragment stored to defragmentation buffer");
        return false;
      }

      frame = FrameFactory.CreateFromIpPacket(defragmentedIpv4Packet, firstTimeStamp);
      return true;
    }
  }
}
