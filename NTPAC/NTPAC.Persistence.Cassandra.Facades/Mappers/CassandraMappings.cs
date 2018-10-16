using System;
using Cassandra.Mapping;
using NTPAC.Persistence.Entities;

namespace NTPAC.Persistence.Cassandra.Facades.Mappers
{
  public class CassandraMappings : Mappings
  {
    public CassandraMappings()
    {
      this.For<CaptureEntity>().TableName(typeof(CaptureEntity).Name).PartitionKey(e => e.Id);

      this.For<L7ConversationEntity>().TableName(typeof(L7ConversationEntity).Name).PartitionKey(e => e.Id)
          .Column(e => e.ProtocolType, c => c.WithName(nameof(L7ConversationEntity.ProtocolType)).WithDbType<Int32>())
          .Column(e => e.Pdus, c => c.WithName(nameof(L7ConversationEntity.Pdus)).AsFrozen())
          .Column(e => e.CaptureId, c => c.WithSecondaryIndex());
    }
  }
}
