using System;

namespace NTPAC.DTO.ConversationTracking
{
  public class L7ConversationListDTO
  {
    public IPEndPointDTO DestinationEndPoint { get; set; }
    public DateTime FirstSeen { get; set; }
    public Guid Id { get; set; }
    public DateTime LastSeen { get; set; }
    public Int32 PduCount { get; set; }
    public IPProtocolType ProtocolType { get; set; }
    public IPEndPointDTO SourceEndPoint { get; set; }
  }
}
