using Microsoft.Extensions.DependencyInjection;
using NTPAC.AkkaSupport;
using Xunit;

namespace NTPAC.Akka.Tests
{
  public class UnitTest1
  {
    private readonly ServiceProvider _services;

    public UnitTest1()
    {
      IServiceCollection serviceCollection = new ServiceCollection();
      ConfigureServices(serviceCollection);
      this._services = serviceCollection.BuildServiceProvider();
    }

    [Fact(Skip = "NotImplemented")]
    public void Test1()
    {
      var akka = this._services.GetService<AkkaSystem>();
    }

    private static void ConfigureServices(IServiceCollection serviceCollection)
    {
      serviceCollection.AddLogging();
      serviceCollection.AddSingleton<AkkaSystem>();
    }
  }
}
