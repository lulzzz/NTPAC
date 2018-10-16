using System;
using UnitOfWork.BaseDataEntity;

namespace NTPAC.Persistence.Entities
{
  public class CaptureEntity : IDataEntity
  {
    public DateTime FirstSeen { get; set; }
    public Int32 L7ConversationCount { get; set; }
    public DateTime LastSeen { get; set; }
    public DateTime Processed { get; set; }
    public String ReassemblerAddress { get; set; } //TODO Why?
    public String Uri { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
  }
}
