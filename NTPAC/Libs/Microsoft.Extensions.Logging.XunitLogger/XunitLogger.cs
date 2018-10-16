using System;
using Xunit.Abstractions;

namespace Microsoft.Extensions.Logging.XunitLogger
{
  public class XunitLogger : ILogger
  {
    private readonly String _categoryName;
    private readonly LogLevel _logLevel;
    private readonly ITestOutputHelper _testOutputHelper;

    public XunitLogger(LogLevel logLevel, ITestOutputHelper testOutputHelper, String categoryName)
    {
      this._logLevel         = logLevel;
      this._testOutputHelper = testOutputHelper;
      this._categoryName     = categoryName;
    }

    private class NoopDisposable : IDisposable
    {
      public static readonly NoopDisposable Instance = new NoopDisposable();

      public void Dispose() { }
    }

    public IDisposable BeginScope<TState>(TState state) => NoopDisposable.Instance;

    public Boolean IsEnabled(LogLevel logLevel) => this._logLevel <= logLevel;

    public void Log<TState>(LogLevel logLevel,
                            EventId eventId,
                            TState state,
                            Exception exception,
                            Func<TState, Exception, String> formatter)
    {
      this._testOutputHelper.WriteLine($"{this._categoryName} [{eventId}] {formatter(state, exception)}");
      if (exception != null)
        this._testOutputHelper.WriteLine(exception.ToString());
    }
  }
}
