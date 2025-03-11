using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace HandlingSupervisor.Middleware
{
  public class RequestLoggingMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
      _next = next;
      _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
      // Логируем входящий запрос
      _logger.LogInformation(
          "Incoming Request: {Method} {Path}",
          context.Request.Method,
          context.Request.Path);

      await _next(context);
    }
  }
}