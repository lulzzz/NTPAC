using System;
using Akka.Actor;
using Akka.Event;
using Akka.Remote;
using NTPAC.ConversatinTracking.Interfaces;
using NTPAC.ConversatinTracking.Interfaces.Models;
using NTPAC.Messages;
using NTPAC.Messages.CaptureTracking;
using NTPAC.Messages.RawPacket;

namespace NTPAC.ConversationTracking.Actors
{
  public class CaptureController
  {
    private readonly IActorRef _captureControllerActor;
    private readonly IActorContext _context;
    private readonly IActorRef _contractor;
    private readonly IRawPacketBatchParserActorFactory _rawPacketBatchParserActorFactory;
    private readonly ReorderingBuffer _reorderingBuffer;
    private Int64 _currentBatchSeqId;
    private Boolean _isCaptureInfoRequested;

    private IActorRef _rawPacketBatchParserActor;

    public CaptureController(IActorContext context,
                             IActorRef contractor,
                             IActorRef captureControllerActor,
                             IRawPacketBatchParserActorFactory rawPacketBatchParserActorFactory)
    {
      this._context                          = context;
      this._contractor                       = contractor;
      this._captureControllerActor           = captureControllerActor;
      this._rawPacketBatchParserActorFactory = rawPacketBatchParserActorFactory;
      this._reorderingBuffer                 = new ReorderingBuffer();
    }

    private ILoggingAdapter Logger => this._context.GetLogger();

    public void OnCaptureInfo(CaptureInfo captureInfo)
    {
      this._rawPacketBatchParserActor =
        this._rawPacketBatchParserActorFactory.Create(this._context, this._captureControllerActor, captureInfo);

      this.ReplayPacketBatchReorderBuffer();
    }

    public void OnProcessRawPacketBatch(RawPacketBatchRequest request)
    {
      var ack = new RawPacketBatchAck {MessageId = request.MessageId};
      this._contractor.Tell(ack, this._captureControllerActor);

      // Is actor spawned for given Sender?
      if (this._rawPacketBatchParserActor == null)
      {
        if (!this._isCaptureInfoRequested)
        {
          this._contractor.Tell(CaptureInfoRequest.Instance);
          this._isCaptureInfoRequested = true;
        }

        this._reorderingBuffer.Store(request);
        return;
      }

      try
      {
        var batchSeqId = request.SeqId;
        if (batchSeqId == this._currentBatchSeqId)
        {
          this.SendBatch(request);

          if (!this._reorderingBuffer.IsEmpty)
          {
            this.ReplayPacketBatchReorderBuffer();
          }
        }
        else if (batchSeqId > this._currentBatchSeqId)
        {
          this._reorderingBuffer.Store(request);
        }
        else
        {
          throw new Exception(
            $"Received batch from the past (possible duplicate). Current {this._currentBatchSeqId}, got {batchSeqId}");
        }
      }
      catch (Exception e)
      {
        this.Logger.Error(e.ToString());
      }
    }


    public void OnCaptureTrackingComplete(CaptureTrackingComplete complete)
    {
      if (this._rawPacketBatchParserActor == null)
      {
        this.Logger.Error("OnCaptureTrackingComplete null");
        return;
      }
      this._rawPacketBatchParserActor.Tell(complete);
    }

    public void OnCaptureTrackingCompleted(CaptureTrackingCompleted completed) { this._contractor.Tell(completed); }

    private void ReplayPacketBatchReorderBuffer()
    {
      foreach (var batch in this._reorderingBuffer)
      {
        if (batch.SeqId != this._currentBatchSeqId)
        {
          break;
        }

        this.SendBatch(batch);
      }
    }

    private void SendBatch(RawPacketBatchRequest batch)
    {
      this._rawPacketBatchParserActor.Tell(batch);
      this._currentBatchSeqId = batch.SeqId+1;
    }

    public Boolean HandlesRawPacketBatchParserActor(IActorRef actor) => this._rawPacketBatchParserActor != null && this._rawPacketBatchParserActor.Path.Equals(actor.Path);
  }
}
