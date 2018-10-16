using System;
using UnitOfWork.BaseDataEntity;

namespace NTPAC.Persistence.Entities
{
  public class L7ConversationEntity : IDataEntity
  {
    public Guid CaptureId { get; set; }
    public IPEndPointEntity DestinationEndPoint { get; set; }
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
    public L7PduEntity[] Pdus { get; set; }
    public Int32 ProtocolType { get; set; }
    public IPEndPointEntity SourceEndPoint { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
  }
}
