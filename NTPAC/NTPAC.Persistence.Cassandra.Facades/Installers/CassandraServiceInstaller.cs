using System;
using System.Collections.Generic;
using Cassandra;
using Cassandra.Mapping;
using Microsoft.Extensions.DependencyInjection;
using NTPAC.Persistence.Cassandra.Facades.Mappers;
using NTPAC.Persistence.Entities;
using UnitOfWork;
using UnitOfWork.CassandraRepository;
using UnitOfWork.CassandraUnitOfWork;
using UnitOfWork.Repository;

namespace NTPAC.Persistence.Cassandra.Facades.Installers
{
  public class CassandraServiceInstaller
  {
    public static void Install(IServiceCollection serviceCollection, String keyspace, params String[] contactPoints)
    {
      serviceCollection.AddSingleton<Mappings, CassandraMappings>();
      serviceCollection.AddSingleton<ICluster>(provider => Cluster.Builder().AddContactPoints(contactPoints).Build());

      serviceCollection.AddSingleton<IUnitOfWork>(provider =>
      {
        var unitOfWork = new CassandraUnitOfWork(provider.GetRequiredService<ICluster>(), provider.GetRequiredService<Mappings>(),
                                                 keyspace);

        // Map UDTs (just UDTs, not entities with their own tables)
        var session = unitOfWork.Session;
        session.UserDefinedTypes.Define(UdtMap.For<L7PduEntity>(), UdtMap.For<IPEndPointEntity>());

        return unitOfWork;
      });

      serviceCollection.AddSingleton<IRepository<CaptureEntity>, BaseRepository<CaptureEntity>>();
      serviceCollection.AddSingleton<IRepository<L7ConversationEntity>, BaseRepository<L7ConversationEntity>>();
      serviceCollection.AddSingleton<IRepositoryWriterAsync<CaptureEntity>>(x => x.GetService<IRepository<CaptureEntity>>());
      serviceCollection.AddSingleton<IRepositoryReaderAsync<CaptureEntity>>(x => x.GetService<IRepository<CaptureEntity>>());
      serviceCollection.AddSingleton<IRepositoryWriterAsync<L7ConversationEntity>>(
        x => x.GetService<IRepository<L7ConversationEntity>>());
      serviceCollection.AddSingleton<IRepositoryReaderAsync<L7ConversationEntity>>(
        x => x.GetService<IRepository<L7ConversationEntity>>());
    }
  }
}
