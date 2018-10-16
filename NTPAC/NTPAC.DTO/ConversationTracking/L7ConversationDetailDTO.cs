using System;
using System.Collections.Generic;

namespace NTPAC.DTO.ConversationTracking
{
  public class L7ConversationDetailDTO
  {
    public IPEndPointDTO DestinationEndPoint { get; set; }
    public DateTime FirstSeen { get; set; }
    public Guid Id { get; set; }
    public DateTime LastSeen { get; set; }
    public IEnumerable<L7PduDTO> Pdus { get; set; }
    public IPProtocolType ProtocolType { get; set; }
    public IPEndPointDTO SourceEndPoint { get; set; }
  }
}
