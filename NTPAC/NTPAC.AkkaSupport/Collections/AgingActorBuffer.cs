using System;
using System.Collections.Generic;
using Akka.Actor;

namespace NTPAC.AkkaSupport.Collections
{
  public class AgingActorBuffer
  {
    private readonly Int64 _inactivityTimeoutTicks;

    private readonly LinkedList<ActorAgeRecord>
      _linkedList = new LinkedList<ActorAgeRecord>(); //LinkedList is faster then Dictionary in this use-case.

    private Int64 _currentTimestampTicks;

    public AgingActorBuffer(Int64 inactivityTimeoutTicks) => this._inactivityTimeoutTicks = inactivityTimeoutTicks;

    public IEnumerable<IActorRef> GetAndRemoveInactiveActors()
    {
      var actorAgeRecordNode = this._linkedList.First;
      while (actorAgeRecordNode?.Value?.IsInactive(this._currentTimestampTicks, this._inactivityTimeoutTicks) ?? false)
      {
        yield return actorAgeRecordNode.Value.Actor;

        var nextNode = actorAgeRecordNode.Next;
        this._linkedList.Remove(actorAgeRecordNode);
        actorAgeRecordNode = nextNode;
      }
    }

    // Checking just first node is sufficient as the list is ordered from the oldest to the youngest
    public Boolean HaveInactiveActors() =>
      this._linkedList.First?.Value?.IsInactive(this._currentTimestampTicks, this._inactivityTimeoutTicks) ?? false;

    public void Update(IActorRef actor, Int64 eventTimestampTicks)
    {
      this._currentTimestampTicks = eventTimestampTicks;

      // Traverse linked list backward (hit is more probable at the end and therefore less steps is required)
      var currentNode = this._linkedList.Last;
      while (currentNode != null)
      {
        var actorAgeRecord = currentNode.Value;
        if (actorAgeRecord.Actor.Path.Uid == actor.Path.Uid)
        {
          // Update timestamp
          actorAgeRecord.UpdateTimestamp(eventTimestampTicks);

          if (!currentNode.Equals(this._linkedList.Last))
          {
            // Move node to the end of the list
            this._linkedList.Remove(currentNode);
            this._linkedList.AddLast(currentNode);
          }

          return;
        }

        currentNode = currentNode.Previous;
      }

      // Actor not found in list, create new record for him
      var newActorAgeRecord = new ActorAgeRecord(actor, eventTimestampTicks);
      this._linkedList.AddLast(newActorAgeRecord);
    }
  }
}
