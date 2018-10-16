using System;

namespace NTPAC.DTO.ConversationTracking
{
  public class L7PduDTO
  {
    public FlowDirection Direction { get; set; }
    public Int64 FirstSeenTicks { get; set; }
    public Int64 LastSeenTicks { get; set; }
    public Byte[] Payload { get; set; }
    public Int32 PayloadLen { get; set; }
  }
}
