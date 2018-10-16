using System;

namespace NTPAC.Messages
{
  public class StreamMessage
  {
    public class Completed
    {
    }

    public class Failure
    {
      public Failure(String reason) => this.Reason = reason;

      public String Reason { get; }
    }

    public class OnInit
    {
      public class Ack
      {
      }
    }
  }
}
