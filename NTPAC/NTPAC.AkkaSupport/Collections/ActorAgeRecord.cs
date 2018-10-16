using System;
using Akka.Actor;

namespace NTPAC.AkkaSupport.Collections
{
  internal class ActorAgeRecord
  {
    public readonly IActorRef Actor;

    public ActorAgeRecord(IActorRef actor, Int64 timestampTicks)
    {
      this.Actor          = actor;
      this.TimestampTicks = timestampTicks;
    }

    public Int64 TimestampTicks { get; private set; }

    public Boolean IsInactive(Int64 currentTimestampTicks, Int64 inactivityTimeoutTicks) =>
      (currentTimestampTicks - this.TimestampTicks) >= inactivityTimeoutTicks;

    public void UpdateTimestamp(Int64 timestampTicks)
    {
      if (timestampTicks < this.TimestampTicks)
      {
        throw new Exception("Updating timestamp to the past");
      }

      this.TimestampTicks = timestampTicks;
    }
  }
}
