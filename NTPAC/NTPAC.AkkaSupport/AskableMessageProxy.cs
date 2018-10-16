using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using NTPAC.AkkaSupport.Interfaces;

namespace NTPAC.AkkaSupport
{
  public class AskableMessageProxy
  {
    private readonly IActorRef _sender;
    
    private readonly ConcurrentDictionary<Int64, TaskCompletionSource<IAskableMessageReply>> _activeTransmissionsTCSs
      = new ConcurrentDictionary<Int64, TaskCompletionSource<IAskableMessageReply>>();

    private Int64 _messageCounter;
    
    public AskableMessageProxy(IActorRef sender ) => this._sender = sender;

    public Task<T> Ask<T>(IActorRef recipient, IAskableMessageRequest message) where T: IAskableMessageReply
    {
      message.MessageId = this.GetNextMessageId(); 
      return this.TellAndStoreTcs<T>(recipient, message);
    }

    protected Task<T> TellAndStoreTcs<T>(IActorRef recipient, IAskableMessageRequest message) where T : IAskableMessageReply
      => this.TellAndStoreTcs<T>(recipient, message, message.MessageId);

    protected Task<T> TellAndStoreTcs<T>(IActorRef recipient, Object message, Int64 messageId) where T: IAskableMessageReply
    {
      recipient.Tell(message, this._sender);
      var tcs = new TaskCompletionSource<IAskableMessageReply>();
      this._activeTransmissionsTCSs[messageId] = tcs;
      return tcs.Task.ContinueWith(receiveTask => (T)receiveTask.Result);
    }
    
    public void MatchReceivedMessage(IAskableMessageReply receivedMessage) 
    {
      if (this._activeTransmissionsTCSs.TryRemove(receivedMessage.MessageId, out var tcs))
      {
        tcs.SetResult(receivedMessage);  
      }
    }
    
    protected Int64 GetNextMessageId() => Interlocked.Increment(ref this._messageCounter);
  }
}
