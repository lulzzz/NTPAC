using System.Threading.Tasks;

namespace NTPAC.Persistence.Cassandra.Migrations
{
  public interface IDbSeed
  {
    Task SeedDb();
  }
}
