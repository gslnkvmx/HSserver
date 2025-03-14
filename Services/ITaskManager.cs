using HandlingSupervisor.Models;

namespace HandlingSupervisor.Services
{
  public interface ITaskManager
  {
    AirportTask? GetTask(int taskId);
    List<AirportTask> GetAllTasks();
    AirportTask CreateTask(AirportTask task);
    AirportTask? UpdateTask(int taskId, AirportTask task);
    bool AssignTask(int taskId, string carId);
    bool CompleteTask(int taskId);

    Task HandleFlightArrivalAsync(string flightId);
    Task HandleFlightStatusChangeAsync(string flightId, string status);
    Task HandleRefuelAsync(string planeId, string planeParking, int fuel);
    Task HandleRefuelTest(string planeId, string planeParking, int fuel);
    Task HandleFollowMeAsync(string planeId, string Runway, string planeParking);
  }
}