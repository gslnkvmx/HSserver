using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;

namespace HandlingSupervisor.Services
{
  public class FileLogger : ILogger
  {
    private readonly string _filePath;
    private static readonly object _lock = new();

    public FileLogger(string path)
    {
      _filePath = path;
      Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
    }

    public IDisposable BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
      if (!IsEnabled(logLevel)) return;

      var message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {formatter(state, exception)}{Environment.NewLine}";

      lock (_lock)
      {
        File.AppendAllText(_filePath, message, Encoding.UTF8);
      }
    }
  }

  public class FileLoggerProvider : ILoggerProvider
  {
    private readonly string _path;

    public FileLoggerProvider(string path)
    {
      _path = path;
    }

    public ILogger CreateLogger(string categoryName) => new FileLogger(_path);

    public void Dispose() { }
  }
}