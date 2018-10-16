using System;
using System.Linq;
using NTPAC.ConversatinTracking.Interfaces.Models;
using NTPAC.Persistence.Entities;

namespace NTPAC.Persistence.Cassandra.Facades.Mappers
{
  public static class L7ConversationMapper
  {
    public static L7ConversationEntity Map(L7Conversation l7Conversation) =>
      new L7ConversationEntity
      {
        DestinationEndPoint = new IPEndPointEntity(l7Conversation.DestinationEndPoint),
        Pdus                = l7Conversation.Pdus.Select(L7PduMapper.Map).ToArray(),
        ProtocolType        = (Int32) l7Conversation.ProtocolType,
        SourceEndPoint      = new IPEndPointEntity(l7Conversation.SourceEndPoint),
        FirstSeen           = l7Conversation.FirstSeen,
        LastSeen            = l7Conversation.LastSeen,
        Id                  = l7Conversation.Id,
        CaptureId           = l7Conversation.CaptureId
      };
  }
}
