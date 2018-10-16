//#define DROP_BATCHES

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using NTPAC.Common.Extensions;
using NTPAC.ConversatinTracking.Interfaces;
using NTPAC.ConversatinTracking.Interfaces.Models;
using NTPAC.Messages.CaptureTracking;
using NTPAC.Messages.RawPacket;

namespace NTPAC.ConversationTracking.Actors
{
  public class CaptureControllerActor : ReceiveActor
  {
    public static readonly String ReassemblerClusterRoleName = "reassembler";
    public static readonly String TypeName = nameof(CaptureControllerActor);
    
    private readonly Dictionary<IActorRef, CaptureController> _contractorToCaptureControllerMap =
      new Dictionary<IActorRef, CaptureController>();

    private readonly ILoggingAdapter _logger = Context.GetLogger();

    private readonly IRawPacketBatchParserActorFactory _rawPacketBatchParserActorFactory;

    public CaptureControllerActor(IRawPacketBatchParserActorFactory rawPacketBatchParserActorFactory)
    {
      this._rawPacketBatchParserActorFactory = rawPacketBatchParserActorFactory;
      this._logger.Info("Spawning CaptureControllerActor");

      this.Receive<RawPacketBatchRequest>(request => this.OnProcessRawPacketBatch(request));
      this.Receive<CaptureTrackingComplete>(complete => this.OnCaptureTrackingComplete(complete));
      this.Receive<CaptureTrackingCompleted>(completed => this.OnCaptureTrackingCompleted(completed));
      this.Receive<CaptureInfo>(captureInfo => this.OnCaptureInfo(captureInfo));
    } 

    public static Props Props(IRawPacketBatchParserActorFactory rawPacketBatchParserActorFactory) =>
      Akka.Actor.Props.Create<CaptureControllerActor>(rawPacketBatchParserActorFactory)
          .WithMailbox("akka.raw-packet-batch-request-priority-mailbox");
    
    private void OnCaptureInfo(CaptureInfo captureInfo)
    {
      if (!this._contractorToCaptureControllerMap.TryGetValue(this.Sender, out var captureController))
      {
        this._logger.Warning("Contractor not found (CaptureInfo) !");
        return;
      }

      captureController.OnCaptureInfo(captureInfo);
    }

    private void OnProcessRawPacketBatch(RawPacketBatchRequest request)
    {
#if DROP_BATCHES
      this.Sender.Tell(new RawPacketBatchAck { MessageId = request.MessageId});
      return;      
#endif
      
      if (!this._contractorToCaptureControllerMap.TryGetValue(this.Sender, out var captureController))
      {
        captureController = new CaptureController(Context, this.Sender, this.Self, this._rawPacketBatchParserActorFactory);
        this._contractorToCaptureControllerMap.Add(this.Sender, captureController);
      }

      captureController.OnProcessRawPacketBatch(request);
    }

    private void OnCaptureTrackingComplete(CaptureTrackingComplete complete)
    {
#if DROP_BATCHES
      this.Sender.Tell(new CaptureTrackingCompleted {MessageId = complete.MessageId});
      return;      
#endif     
      
      if (!this._contractorToCaptureControllerMap.TryGetValue(this.Sender, out var captureController))
      {
        this._logger.Info("Contractor not found (CaptureTrackingComplete)!");
        // Respond to received CaptureTrackingComplete as contractor who sent it will await the response
        this.Sender.Tell(new CaptureTrackingCompleted {MessageId = complete.MessageId});
        return;
      }

      captureController.OnCaptureTrackingComplete(complete);
    }

    private void OnCaptureTrackingCompleted(CaptureTrackingCompleted completed)
    {
      var captureController = this._contractorToCaptureControllerMap.Values.FirstOrDefault(cc => cc.HandlesRawPacketBatchParserActor(this.Sender));
      if (captureController == null) {
        this._logger.Warning("CaptureController not found (CaptureTrackingCompleted)!");
        return;
      }

      captureController.OnCaptureTrackingCompleted(completed);
      this._contractorToCaptureControllerMap.RemoveSingleReferenceValue(captureController);
    }
  }
}
