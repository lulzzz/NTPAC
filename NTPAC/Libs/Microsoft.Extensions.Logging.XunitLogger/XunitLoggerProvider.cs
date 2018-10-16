using System;
using Xunit.Abstractions;

namespace Microsoft.Extensions.Logging.XunitLogger
{
  public class XunitLoggerProvider : ILoggerProvider
  {
    private readonly LogLevel _logLevel;
    private readonly ITestOutputHelper _testOutputHelper;

    public XunitLoggerProvider(LogLevel logLevel, ITestOutputHelper testOutputHelper)
    {
      this._logLevel         = logLevel;
      this._testOutputHelper = testOutputHelper;
    }

    public void Dispose() { }

    public ILogger CreateLogger(String categoryName) => new XunitLogger(this._logLevel, this._testOutputHelper, categoryName);
  }
}
