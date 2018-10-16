using NTPAC.ConversatinTracking.Interfaces.Models;
using NTPAC.Persistence.Entities;

namespace NTPAC.Persistence.Cassandra.Facades.Mappers
{
  public static class CaptureMapper
  {
    public static CaptureEntity Map(Capture capture) =>
      new CaptureEntity
      {
        Uri                 = capture.Info.UriString,
        Processed           = capture.Processed,
        ReassemblerAddress  = capture.ReassemblerAddress,
        Id                  = capture.Id,
        L7ConversationCount = capture.L7ConversationCount,
        FirstSeen           = capture.FirstSeen,
        LastSeen            = capture.LastSeen
      };
  }
}
