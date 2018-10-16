using Microsoft.Extensions.DependencyInjection;
using NTPAC.Persistence.Entities;
using UnitOfWork;
using UnitOfWork.InMemoryRepository;
using UnitOfWork.InMemoryUnitOfWork;
using UnitOfWork.Repository;

namespace NTPAC.Persistence.InMemory.Facades
{
  public class InMemoryServiceInstaller
  {
    public static void Install(IServiceCollection serviceCollection)
    {
      serviceCollection.AddSingleton<IUnitOfWork, InMemoryUnitOfWork>();
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
