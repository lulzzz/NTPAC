using System;
using MessagePack;
using NTPAC.AkkaSupport.Interfaces;

namespace NTPAC.Messages.CaptureTracking
{
  [MessagePackObject]
  public class CaptureTrackingComplete : IAskableMessageRequest
  {
    [Key(0)]
    public Int64 MessageId { get; set; }
  }
}
