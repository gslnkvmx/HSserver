using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HandlingSupervisor.Services
{
  public class BoardInfoResponse
  {
    [JsonPropertyName("plane_id")]
    public required string PlaneId { get; set; }

    public required FlightInfo Flight { get; set; }

    public List<int>? Baggage { get; set; }

    [JsonPropertyName("currentFuel")]
    public double CurrentFuel { get; set; }

    [JsonPropertyName("minRequiredFuel")]
    public double MinRequiredFuel { get; set; }

    [JsonPropertyName("maxFuel")]
    public double MaxFuel { get; set; }

    [JsonPropertyName("maxCapacity")]
    public int MaxCapacity { get; set; }

    [JsonPropertyName("numPassengers")]
    public int NumPassengers { get; set; }

    public class FlightInfo
    {
      [JsonPropertyName("flight_id")]
      public string? FlightId { get; set; }
    }
  }

  public interface IBoardInfoService
  {
    Task<BoardInfoResponse?> GetBoardInfoAsync(string planeId);
  }

  public class BoardInfoService : IBoardInfoService
  {
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ConsoleLogger _consoleLogger;
    private readonly FileLogger _fileLogger;

    public BoardInfoService(HttpClient httpClient, ConsoleLogger consoleLogger, FileLogger fileLogger)
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

    public async Task<BoardInfoResponse?> GetBoardInfoAsync(string planeId)
    {
      try
      {
        var response = await _httpClient.GetAsync($"v1/board/{planeId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
          _consoleLogger.PrintOverlay($"Адрес {_httpClient.BaseAddress}v1/board/{planeId} не доступен!");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        _fileLogger.LogInformation(content);

        var fullResponse = JsonSerializer.Deserialize<BoardInfoResponse>(content, _jsonOptions);

        return fullResponse;
      }
      catch
      {
        _consoleLogger.PrintOverlay($"Адрес Адрес {_httpClient.BaseAddress}v1/board/{planeId} не доступен!");
        return null;
      }
    }
  }
}