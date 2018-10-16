using System;
using Cassandra;
using Cassandra.Mapping;
using Microsoft.Extensions.DependencyInjection;
using NTPAC.Persistence.Cassandra.Entities;
using UnitOfWork;
using UnitOfWork.CassandraUnitOfWork;
using UnitOfWork.Repository;

namespace NTPAC.Persistence.Cassandra.Facades
{
  public class CassandraServiceInstaller
  {
    public static void Install(IServiceCollection serviceCollection, String contactPoint, String keyspace)
    {
      serviceCollection.AddSingleton<Mappings, CassandraMappings>();
      serviceCollection.AddSingleton<ICluster>(provider => Cluster.Builder().AddContactPoint((String) contactPoint).Build());
           
      //serviceCollection.AddSingleton<IUnitOfWork, CassandraUnitOfWork>();
      serviceCollection.AddSingleton<IUnitOfWork>(provider =>
      {
        var unitOfWork = new CassandraUnitOfWork(provider.GetRequiredService<ICluster>(), provider.GetRequiredService<Mappings>(), keyspace);
        
        // Map UDTs
        var session = unitOfWork.Session;
        session.UserDefinedTypes.Define(
          UdtMap.For<CaptureEntity>(),
          UdtMap.For<L7PduEntity>(),
          UdtMap.For<IPEndPointEntity>(),
          UdtMap.For<L7ConversationEntity>()
        );
        
        return unitOfWork;
      });

      serviceCollection.AddSingleton<IRepositoryWriterAsync<CaptureEntity>, UnitOfWork.CassandraRepository.BaseRepository<CaptureEntity>>();
      serviceCollection.AddSingleton<IRepositoryWriterAsync<L7ConversationEntity>, UnitOfWork.CassandraRepository.BaseRepository<L7ConversationEntity>>();
    }
  }
}
