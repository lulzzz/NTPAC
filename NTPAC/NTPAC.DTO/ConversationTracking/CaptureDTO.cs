using System;

namespace NTPAC.DTO.ConversationTracking
{
  public class CaptureListDTO
  {
    public TimeSpan AnalysisDuration { get; set; }
    public Int64 CaptureSize { get; set; }
    public DateTime FirstSeen { get; set; }
    public Guid Id { get; set; }
    public Int64 L7ConversationCount { get; set; }
    public DateTime LastSeen { get; set; }
    public DateTime Processed { get; set; }
    public String Uri { get; set; }
  }
}
