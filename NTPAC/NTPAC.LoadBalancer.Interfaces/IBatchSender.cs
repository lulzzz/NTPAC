using System;
using Akka.Actor;
using NTPAC.LoadBalancer.Messages;

namespace NTPAC.LoadBalancer.Interfaces
{
  public interface IBatchSender
  {
    UInt64 DistributedPackets { get; }
    void Complete();
    void Initialize(IActorRef self, IActorRef rawPacketParserActor);
    void SendBatch(PacketBatch packetBatch);
  }
}
