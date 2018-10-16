using Microsoft.Extensions.DependencyInjection;
using NTPAC.Persistence.Entities;
using UnitOfWork;
using UnitOfWork.DevnullRepository;
using UnitOfWork.DevnullUnitOfWork;
using UnitOfWork.Repository;

namespace NTPAC.Persistence.DevNull.Facades
{
  public class DevNullServiceInstaller
  {
    public static void Install(IServiceCollection serviceCollection)
    {
      serviceCollection.AddSingleton<IUnitOfWork, DevnullUnitOfWork>();
      serviceCollection.AddSingleton<IRepositoryWriterAsync<CaptureEntity>, BaseRepository<CaptureEntity>>();
      serviceCollection.AddSingleton<IRepositoryReaderAsync<CaptureEntity>, BaseRepository<CaptureEntity>>();
      serviceCollection.AddSingleton<IRepositoryWriterAsync<L7ConversationEntity>, BaseRepository<L7ConversationEntity>>();
      serviceCollection.AddSingleton<IRepositoryReaderAsync<L7ConversationEntity>, BaseRepository<L7ConversationEntity>>();
    }
  }
}
