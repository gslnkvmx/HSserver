using System.Text.Json.Serialization;

namespace HandlingSupervisor.Models
{
  public enum TaskState { Sent, Assigned, InProgress, Completed }
  public enum TaskType { pickUpPassengers, deliverPassengers, pickUpBaggage, deliverFood, deliverBaggage }

  public class RefuelTaskRequest
  {
    [JsonPropertyName("planeId")]
    public string PlaneId { get; set; }
    [JsonPropertyName("planeParking")]
    public string PlaneParking { get; set; }
    [JsonPropertyName("fuelAmount")]
    public int FuelAmount { get; set; }
  }

  public class FollowMeTaskRequest
  {
    public string PlaneId { get; set; }
    public string Runway { get; set; }
    public string PlaneParking { get; set; }
  }

  public class AirportTask
  {
    [JsonPropertyName("taskId")]
    public int TaskId { get; set; }
    [JsonPropertyName("taskType")]
    public string TaskType { get; set; }
    [JsonPropertyName("state")]
    public TaskState State { get; set; } = TaskState.Sent;
    [JsonPropertyName("stateMessage")]
    public string? StateMessage { get; set; }
    [JsonPropertyName("planeId")]
    public string? PlaneId { get; set; }
    [JsonPropertyName("flightId")]
    public string? FlightId { get; set; }
    [JsonPropertyName("carId")]
    public string? CarId { get; set; }
    [JsonPropertyName("point")]
    public string? Point { get; set; }
    [JsonPropertyName("details")]
    public object? Details { get; set; }
  }

  public class TaskAssignment
  {
    public required string CarId { get; set; }
  }
}