using System;
using UnitOfWork.BaseDataEntity;

namespace NTPAC.Persistence.Entities
{
  public class L7PduEntity : IDataEntity
  {
    public SByte Direction { get; set; }
    public Int64 FirstSeenTicks { get; set; }
    public Int64 LastSeenTicks { get; set; }
    public Byte[] Payload { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
  }
}
