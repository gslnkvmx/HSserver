using HandlingSupervisor.Models;
using System.Collections.Concurrent;

namespace HandlingSupervisor.Services
{
  public class ConsoleLogger : ILogger
  {
    private readonly ConcurrentDictionary<int, int> _taskLineMap = new();
    private int _lastPrintedLine = 0;
    private readonly object _consoleLock = new();

    public IDisposable BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
      // Этот метод будет использоваться для стандартного логгирования
      // Мы его не используем, так как у нас кастомный вывод
    }

    public void UpdateTaskLog(AirportTask task)
    {
      lock (_consoleLock)
      {
        if (!_taskLineMap.TryGetValue(task.TaskId, out var line))
        {
          line = _lastPrintedLine++;
          _taskLineMap[task.TaskId] = line;
        }

        var origRow = Console.CursorTop;
        var origCol = Console.CursorLeft;

        try
        {
          Console.SetCursorPosition(0, line);
          Console.Write(new string(' ', Console.WindowWidth - 1));
          Console.SetCursorPosition(0, line);

          var statusColor = task.State switch
          {
            TaskState.Sent => ConsoleColor.DarkGray,
            TaskState.Assigned => ConsoleColor.Blue,
            TaskState.InProgress => ConsoleColor.Yellow,
            TaskState.Completed => ConsoleColor.Green,
            _ => ConsoleColor.White
          };

          Console.Write($" {task.TaskId}".FixedWidth(12));
          Console.Write($"│ {task.FlightId}".FixedWidth(16));
          Console.Write($"│ ✈️{task.PlaneId}".FixedWidth(16));
          Console.Write("│");
          Console.ForegroundColor = statusColor;
          Console.Write($" {task.State.ToString()}".FixedWidth(10));
          Console.ResetColor();
          Console.Write($"│ 🚗 {task.CarId?.ToString() ?? " --- "}".FixedWidth(15));
          Console.Write($"│ 📍 {task.Point}".FixedWidth(13));
          Console.Write($"│ 📝 {task.StateMessage?.ToString() ?? ""}".FixedWidth(30));

          if (task.State == TaskState.Completed)
          {
            _taskLineMap.TryRemove(task.TaskId, out _);
          }
        }
        finally
        {
          Console.SetCursorPosition(origCol, origRow);
        }
      }
    }

    public void PrintHeader()
    {
      System.Console.WriteLine();
      Console.WriteLine(" Active Tasks ══════════════╪═══════════════╪══════════╪══════════════╪════════════╪═══════════════════════════════════");
      Console.WriteLine(" ID         │ Flight        │ Plane         │ Status   │ Car          │ Point      │ Description");
      Console.WriteLine("════════════╪═══════════════╪═══════════════╪══════════╪══════════════╪════════════╪═══════════════════════════════════");
      _lastPrintedLine = Console.CursorTop;
    }

    public void PrintOverlay(string message)
    {
      lock (_consoleLock)
      {
        var origTop = Console.CursorTop;
        var origLeft = Console.CursorLeft;

        Console.SetCursorPosition(0, 0);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, 0);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write($"[WARNING] {message.FixedWidth(100)}");
        Console.ResetColor();

        Console.SetCursorPosition(origLeft, origTop);
      }
    }
  }

  public static class StringExtensions
  {
    public static string FixedWidth(this string value, int width, bool alignRight = false)
    {
      value ??= string.Empty; // Обработка null

      if (value.Length > width)
        return value.Substring(0, width);

      return alignRight
          ? value.PadLeft(width)
          : value.PadRight(width);
    }
  }
}