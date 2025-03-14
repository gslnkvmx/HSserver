using HandlingSupervisor.Models;
using HandlingSupervisor.Services;

public static class TaskEndpoints
{
  public static void MapTaskEndpoints(this WebApplication app)
  {

    app.MapPut("/v1/tasks/{taskId}", (int taskId, AirportTask task, ITaskManager manager) =>
    {
      var updatedTask = manager.UpdateTask(taskId, task);
      return updatedTask != null ? Results.Ok(updatedTask) : Results.NotFound();
    });

    app.MapPut("/v1/tasks/{taskId}/assign", (int taskId, TaskAssignment assignment, ITaskManager manager) =>
    {
      return manager.AssignTask(taskId, assignment.CarId)
          ? Results.Ok()
          : Results.NotFound();
    });

    app.MapPut("/v1/tasks/{taskId}/complete", (int taskId, ITaskManager manager) =>
    {
      return manager.CompleteTask(taskId)
          ? Results.Ok()
          : Results.NotFound();
    });

    app.MapGet("/v1/tasks/{taskId}/info", (int taskId, ITaskManager manager) =>
    {
      var task = manager.GetTask(taskId);
      return task != null ? Results.Ok(task) : Results.NotFound();
    });

    app.MapGet("/v1/tasks/info", (ITaskManager manager) =>
    {
      return Results.Ok(manager.GetAllTasks());
    });

    app.MapPost("/v1/tasks/refuel", (RefuelTaskRequest request, ITaskManager manager, ConsoleLogger _consoleLogger) =>
    {
      _consoleLogger.PrintOverlay($"{request.PlaneId}, {request.PlaneParking}, {request.FuelAmount}");
      manager.HandleRefuelTest(request.PlaneId, request.PlaneParking, request.FuelAmount);
      return Results.Ok();
    });

    app.MapPost("/v1/tasks/followMe", (FollowMeTaskRequest request, ITaskManager manager) =>
    {
      manager.HandleFollowMeAsync(request.PlaneId, request.Runway, request.PlaneParking);
      return Results.Ok();
    });
  }
}