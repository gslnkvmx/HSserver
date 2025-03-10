using System.Net;
using System.Text.Json;

namespace HandlingSupervisor.Services
{
  public class FlightInfoResponse
  {
    public required string FlightId { get; set; }
    public required string PlaneId { get; set; }
    public required string Type { get; set; }  // "depart" или "arrive"
    public required string Status { get; set; }
    public required string Gate { get; set; }
    public required string PlaneParking { get; set; }
    public string? Runway { get; set; }
  }

  public interface IFlightInfoService
  {
    Task<FlightInfoResponse?> GetFlightInfoAsync(string flightId);
  }

  public class FlightInfoService : IFlightInfoService
  {
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ConsoleLogger _consoleLogger;
    private readonly FileLogger _fileLogger;

    public FlightInfoService(HttpClient httpClient, ConsoleLogger consoleLogger, FileLogger fileLogger)
    {
      _httpClient = httpClient;
      _httpClient.Timeout = TimeSpan.FromSeconds(3);

      _jsonOptions = new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true
      };

      _consoleLogger = consoleLogger;
      _fileLogger = fileLogger;
    }

    public async Task<FlightInfoResponse?> GetFlightInfoAsync(string flightId)
    {
      try
      {
        var response = await _httpClient.GetAsync($"v1/flights/{flightId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
          _consoleLogger.PrintOverlay($"Адрес {_httpClient.BaseAddress}v1/flights/{flightId} не доступен!");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        _fileLogger.LogInformation(content);
        var fullResponse = JsonSerializer.Deserialize<FlightInfoResponse>(content, _jsonOptions);

        return fullResponse;
      }
      catch
      {
        _consoleLogger.PrintOverlay($"Адрес {_httpClient.BaseAddress}v1/flights/{flightId} не доступен!");
        return null;
      }
    }
  }
}