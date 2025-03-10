using System.Text.Json.Serialization;

namespace HandlingSupervisor.Models
{
  public class FlightStatusMessage
  {
    [JsonPropertyName("flightId")]
    public required string FlightId { get; set; }
    [JsonPropertyName("status")]
    public required string Status { get; set; }
  }
}