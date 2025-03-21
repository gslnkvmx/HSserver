using HandlingSupervisor.Middleware;
using HandlingSupervisor.Models;
using HandlingSupervisor.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.Clear();

var builder = WebApplication.CreateBuilder(args);

var consoleLogger = new ConsoleLogger();
var fileLogger = new FileLogger("logs/app.log");

// Регистрируем провайдеры
builder.Logging
    .ClearProviders()
    .AddProvider(new FileLoggerProvider("logs/app.log"))
    .AddProvider(new ConsoleLoggerProvider(consoleLogger))
    .AddFilter<ConsoleLoggerProvider>(level => level >= LogLevel.Warning);

// Регистрируем ConsoleLogger как сервис
builder.Services.AddSingleton(consoleLogger);
builder.Services.AddSingleton(fileLogger);

builder.Services.AddHttpClient<IFlightInfoService, FlightInfoService>(client =>
{
  client.BaseAddress = new Uri("https://social-jokes-design.loca.lt");
});

builder.Services.AddHttpClient<IBoardInfoService, BoardInfoService>(client =>
{
  client.BaseAddress = new Uri("https://every-emus-accept.loca.lt");
});

builder.Services.AddSingleton<ITaskManager, BaseTaskManager>();
builder.Services.AddSingleton<RabbitMQService>();
builder.Services.AddHostedService<RabbitMQConsumerService>();

var app = builder.Build();

app.MapTaskEndpoints();

PrintEndpoints(app);

app.UseMiddleware<RequestLoggingMiddleware>();

app.Run();

void PrintEndpoints(WebApplication app)
{
  Console.WriteLine("\n\nДоступные эндпоинты:");
  var endpoints = app as IEndpointRouteBuilder;
  foreach (var endpoint in endpoints.DataSources
      .SelectMany(ds => ds.Endpoints)
      .OfType<RouteEndpoint>())
  {
    var method = endpoint.Metadata
        .OfType<HttpMethodMetadata>()
        .FirstOrDefault()?
        .HttpMethods
        .FirstOrDefault() ?? "ANY";

    var route = endpoint.RoutePattern.RawText;
    Console.WriteLine($"{method} {route}");
  }
}