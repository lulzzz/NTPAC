using System;
using Akka.Event;
using Microsoft.Extensions.Logging;
using LogLevel = Akka.Event.LogLevel;

namespace NTPAC.AkkaSupport
{
  public class AkkaLoggingAdapter<T> : ILogger<T>
  {
    private readonly ILoggingAdapter _logger;
    public AkkaLoggingAdapter(ILoggingAdapter logger) => this._logger = logger;

    private static LogLevel MapLogLevel(Microsoft.Extensions.Logging.LogLevel logLevel)
    {
      switch (logLevel)
      {
        case Microsoft.Extensions.Logging.LogLevel.Trace:
          return LogLevel.DebugLevel;
        case Microsoft.Extensions.Logging.LogLevel.Debug:
          return LogLevel.DebugLevel;
        case Microsoft.Extensions.Logging.LogLevel.Information:
          return LogLevel.InfoLevel;
        case Microsoft.Extensions.Logging.LogLevel.Warning:
          return LogLevel.WarningLevel;
        case Microsoft.Extensions.Logging.LogLevel.Error:
          return LogLevel.ErrorLevel;
        case Microsoft.Extensions.Logging.LogLevel.Critical:
          return LogLevel.ErrorLevel;
        case Microsoft.Extensions.Logging.LogLevel.None:
          return LogLevel.ErrorLevel;
        default:
          return LogLevel.ErrorLevel;
      }
    }

    public IDisposable BeginScope<TState>(TState state) => null;

    public Boolean IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => this._logger.IsEnabled(MapLogLevel(logLevel));

    public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel,
                            EventId eventId,
                            TState state,
                            Exception exception,
                            Func<TState, Exception, String> formatter)
    {
      this._logger.Log(MapLogLevel(logLevel), formatter.Invoke(state, exception));
    }
  }
}
