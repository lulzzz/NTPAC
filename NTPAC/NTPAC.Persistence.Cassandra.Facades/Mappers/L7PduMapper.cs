using System;
using NTPAC.ConversatinTracking.Interfaces.Models;
using NTPAC.Persistence.Entities;

namespace NTPAC.Persistence.Cassandra.Facades.Mappers
{
  public static class L7PduMapper
  {
    public static L7PduEntity Map(L7Pdu l7Pdu) =>
      new L7PduEntity
      {
        FirstSeenTicks = l7Pdu.FirstSeenTicks,
        LastSeenTicks  = l7Pdu.LastSeenTicks,
        Direction      = (SByte) l7Pdu.Direction,
        Payload        = l7Pdu.Payload
      };
  }
}
