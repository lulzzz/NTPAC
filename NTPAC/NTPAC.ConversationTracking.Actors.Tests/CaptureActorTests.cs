using Akka.TestKit.Xunit2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.XunitLogger;
using Xunit.Abstractions;

namespace NTPAC.ConversationTracking.Actors.Tests
{
  public sealed class CaptureActorTests : TestKit
  {
    private ServiceProvider _services;

    public CaptureActorTests(ITestOutputHelper output)
    {
      IServiceCollection serviceCollection = new ServiceCollection();
      this.ConfigureServices(serviceCollection);
      this._services = serviceCollection.BuildServiceProvider();
      this._services.GetService<ILoggerFactory>().AddProvider(new XunitLoggerProvider(LogLevel.Debug, output));
    }

    private void ConfigureServices(IServiceCollection serviceCollection)
    {
      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(serviceCollection);
    }
  }
}
