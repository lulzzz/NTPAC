using System;
using Akka.Actor;
using NTPAC.LoadBalancer.Interfaces;
using NTPAC.LoadBalancer.Messages;
using NTPAC.Messages.CaptureTracking;
using NTPAC.Messages.RawPacket;

namespace NTPAC.LoadBalancer.Actors.Offline
{
  public class BatchSender : IBatchSender
  {
    private Int64 _batchSeqNum;
    private IActorRef _loadBalancerActor;
    private IActorRef _rawPacketParserActor;
    private Int64 _messageCounter = 1;
    public UInt64 DistributedPackets { get; private set; }

    public void Complete() { this._rawPacketParserActor.Tell(new CaptureTrackingComplete {MessageId = this._messageCounter++}); }

    public void Initialize(IActorRef loadBalancerActor, IActorRef rawPacketParserActor)
    {
      this._loadBalancerActor    = loadBalancerActor;
      this._rawPacketParserActor = rawPacketParserActor;
    }

    public void SendBatch(PacketBatch packetBatch)
    {
      var batchRequest = new RawPacketBatchRequest(packetBatch.Batch, this._batchSeqNum++) {MessageId = this._messageCounter++};
      this.DistributedPackets += (UInt64) packetBatch.Count;

      this._rawPacketParserActor.Tell(batchRequest, this._loadBalancerActor);
    }
  }
}
