using Microsoft.Extensions.Logging;
using System;

namespace HandlingSupervisor.Services
{
  public class ConsoleLoggerProvider : ILoggerProvider
  {
    private readonly ConsoleLogger _consoleLogger;

    public ConsoleLoggerProvider(ConsoleLogger consoleLogger)
    {
      _consoleLogger = consoleLogger;
    }

    public ILogger CreateLogger(string categoryName)
    {
      return _consoleLogger;
    }

    public void Dispose()
    {
      // Очистка ресурсов, если необходимо
    }
  }
}