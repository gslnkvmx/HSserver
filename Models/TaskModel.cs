namespace HandlingSupervisor.Models
{
  public enum TaskState { Sent, Assigned, InProgress, Completed }
  public enum TaskType { pickUpPassengers, deliverPassengers, pickUpBaggage, deliverFood, deliverBaggage }

  public class AirportTask
  {
    public int TaskId { get; set; }
    public TaskType TaskType { get; set; }
    public TaskState State { get; set; } = TaskState.Sent;
    public string? StateMessage { get; set; }
    public string? PlaneId { get; set; }
    public string? FlightId { get; set; }
    public string? CarId { get; set; }
    public string? Point { get; set; }
    public object? Details { get; set; }
  }

  public class TaskAssignment
  {
    public required string CarId { get; set; }
  }
}