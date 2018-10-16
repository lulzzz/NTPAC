using System;
using System.Diagnostics;
using Akka.Actor;
using Akka.Event;
using NTPAC.ConversatinTracking.Interfaces;
using NTPAC.ConversatinTracking.Interfaces.Models;
using NTPAC.LoadBalancer.Actors.Enums;
using NTPAC.LoadBalancer.Interfaces;
using NTPAC.LoadBalancer.Messages;
using NTPAC.Messages.CaptureTracking;
using NTPAC.Messages.RawPacket;

namespace NTPAC.LoadBalancer.Actors
{
  public class OfflineLoadBalancerActor : FSM<OfflineLoadBalancerStates, PacketBatch>
  {
    private readonly IBatchLoader _batchLoader;
    private readonly IBatchSender _batchSender;
    private readonly LoadBalancerSettings _loadBalancerSettings;
    private readonly ILoggingAdapter _logger = Context.GetLogger();
    private readonly IRawPacketBatchParserActorFactory _rawPacketBatchParserActorFactory;
    private CaptureInfo _captureInfo;
    private IActorRef _contractor;
    private IActorRef _rawPacketBatchParserActor;
    private readonly Stopwatch _totalSw = new Stopwatch();

    public OfflineLoadBalancerActor(IBatchLoader batchLoader,
                                    IBatchSender batchSender,
                                    LoadBalancerSettings loadBalancerSettings,
                                    IRawPacketBatchParserActorFactory rawPacketBatchParserActorFactory)
    {
      this._batchLoader                      = batchLoader;
      this._loadBalancerSettings             = loadBalancerSettings;
      this._rawPacketBatchParserActorFactory = rawPacketBatchParserActorFactory;
      this._batchSender                      = batchSender;

      this.StartWith(OfflineLoadBalancerStates.WaitingForProcessingRequest, null);

      this.SetupFSMWhen();
      this.SetupFSMTransitions();
      this.SetupPeriodicalGarbageCollecting();
    }


    public static Props Props(IBatchLoader batchLoader,
                              IBatchSender batchSender,
                              LoadBalancerSettings loadBalancerSettings,
                              IRawPacketBatchParserActorFactory rawPacketBatchParserActorFactory) =>
      Akka.Actor.Props.Create(
        () => new OfflineLoadBalancerActor(batchLoader, batchSender, loadBalancerSettings,
                                           rawPacketBatchParserActorFactory));

    private void RunPeriodicalGarbageCollecting()
    {
      this._logger.Info("Starting periodical GarbageCollecting");
      GC.Collect();
    }

    private void SetupFSMFinalized() { this.When(OfflineLoadBalancerStates.Finalized, state => null); }

    private void SetupFSMFinalizing()
    {
      this.When(OfflineLoadBalancerStates.Finalizing, state =>
      {
        if (state.FsmEvent is CaptureTrackingCompleted)
        {
          return this.GoTo(OfflineLoadBalancerStates.Finalized);
        }

        return null;
      });
    }

    private void SetupFSMLoadingBatch()
    {
      this.When(OfflineLoadBalancerStates.LoadingBatch, state =>
      {
        switch (state.FsmEvent)
        {
          case PacketBatch packetBatch when packetBatch.IsEmpty:
            return this.GoTo(OfflineLoadBalancerStates.Finalizing);
          case PacketBatch packetBatch:
            return this.GoTo(OfflineLoadBalancerStates.SendingBatch).Using(packetBatch);
        }

        return null;
      });
    }

    private void SetupFSMSendingBatch()
    {
      //TODO handle ACK lost
      this.When(OfflineLoadBalancerStates.SendingBatch, state =>
      {
        if (state.FsmEvent is RawPacketBatchAck)
        {
          return this.GoTo(OfflineLoadBalancerStates.LoadingBatch);
        }

        return null;
      });
    }

    private void SetupFSMTransitions()
    {
      this.OnTransition((state, nextState) =>
      {
        switch (nextState)
        {
          case OfflineLoadBalancerStates.LoadingBatch:
            this._batchLoader.LoadBatch(this._loadBalancerSettings.BatchSize).PipeTo(this.Self);
            break;
          case OfflineLoadBalancerStates.SendingBatch:
            this._batchSender.SendBatch(this.NextStateData);
            break;
          case OfflineLoadBalancerStates.Finalizing:
            this._batchSender.Complete();
            break;
          case OfflineLoadBalancerStates.Finalized:
            this._totalSw.Stop();
            var processingResult = new ProcessingResult
                                   {
                                     Success = true,
                                     ProcessedPackets = this._batchSender.DistributedPackets,
                                     TotalTime = this._totalSw.Elapsed,
                                     CaptureSize = this._batchLoader.CaptureSize
                                   };
            this._contractor.Tell(processingResult);
            break;
        }
      });
    }


    private void SetupFSMUnhandled()
    {
      this.WhenUnhandled(state =>
      {
        if (state.FsmEvent is CollectGarbageRequest _)
        {
          this.RunPeriodicalGarbageCollecting();
          return this.Stay();
        }

        this._logger.Error($"Unexpected message received. Current FSM state:{this.StateName}, event {state.FsmEvent}");

        return null;
      });
    }

    private void SetupFSMWaitingForProcessingRequest()
    {
      this.When(OfflineLoadBalancerStates.WaitingForProcessingRequest, state =>
      {
        if (state.FsmEvent is CaptureProcessingRequest processingRequest)
        {
          this._captureInfo = processingRequest.CaptureInfo;
          this._contractor  = this.Sender;

          this._rawPacketBatchParserActor = this._rawPacketBatchParserActorFactory.Create(Context, this.Self, this._captureInfo);

          this._batchSender.Initialize(this.Self, this._rawPacketBatchParserActor);
          this._batchLoader.Open(this._captureInfo.Uri);
          
          this._totalSw.Start();

          return this.GoTo(OfflineLoadBalancerStates.LoadingBatch);
        }

        return null;
      });
    }

    private void SetupFSMWhen()
    {
      this.SetupFSMWaitingForProcessingRequest();

      this.SetupFSMLoadingBatch();

      this.SetupFSMSendingBatch();

      this.SetupFSMFinalizing();

      this.SetupFSMFinalized();

      this.SetupFSMUnhandled();
    }


    private void SetupPeriodicalGarbageCollecting()
    {
      Context.System.Scheduler.ScheduleTellRepeatedly(this._loadBalancerSettings.PeriodicalGarbageCollectingInitial,
                                                      this._loadBalancerSettings.PeriodicalGarbageCollectingPeriod, this.Self,
                                                      CollectGarbageRequest.Instance, this.Self);
    }
  }
}
