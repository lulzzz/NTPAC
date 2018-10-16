using System;
using System.Threading.Tasks;
using Akka.Actor;
using NTPAC.AkkaSupport;
using NTPAC.AkkaSupport.Interfaces;
using NTPAC.Messages.Sharding;

namespace NTPAC.LoadBalancer.Actors.Online
{
  public class AskableReassemblerEntityMessageProxy : AskableMessageProxy
  {
    private readonly IActorRef _clusterProxy;
    
    public AskableReassemblerEntityMessageProxy(IActorRef sender, IActorRef clusterProxy) : base(sender)
      => this._clusterProxy = clusterProxy;

    public Task<T> Ask<T>(Int32 entityId, IAskableMessageRequest message) where T: IAskableMessageReply
    {
      message.MessageId = this.GetNextMessageId(); 
      var messageEnvelope = new ReassemblerEntityMessageEnvelope(entityId, message);
      return this.TellAndStoreTcs<T>(this._clusterProxy, messageEnvelope, message.MessageId);
    }
  }
}
