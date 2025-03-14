using System.Numerics;
using System.Threading.Tasks;
using HandlingSupervisor.Models;

namespace HandlingSupervisor.Services
{
  public class BaseTaskManager : ITaskManager
  {
    private readonly Dictionary<int, AirportTask> _tasks = new();
    private int _currentId = 1;
    private readonly RabbitMQService _rabbitMQService;
    private readonly ConsoleLogger _consoleLogger;
    private readonly FileLogger _fileLogger;
    private readonly IFlightInfoService _flightInfoService;
    private readonly IBoardInfoService _boardInfoService;

    public BaseTaskManager(RabbitMQService rabbitMQService, ConsoleLogger consoleLogger, IFlightInfoService flightInfoService, IBoardInfoService boardInfoService, FileLogger fileLogger)
    {
      _rabbitMQService = rabbitMQService;
      _consoleLogger = consoleLogger;
      _fileLogger = fileLogger;
      _consoleLogger.PrintHeader();
      _flightInfoService = flightInfoService;
      _boardInfoService = boardInfoService;
    }

    public async Task HandleFlightArrivalAsync(string flightId)
    {
      try
      {
        var flightInfo = await _flightInfoService.GetFlightInfoAsync(flightId);
        if (flightInfo == null) throw new Exception("Can't get flight data");
        var boardInfo = await _boardInfoService.GetBoardInfoAsync(flightInfo.PlaneId);
        if (boardInfo == null) throw new Exception("Can't get flight data");
        // Создаем задачу забрать пассажиров для passengerBus
        var task1 = new AirportTask
        {
          TaskId = _currentId++,
          TaskType = "pickUpPassengers",
          State = TaskState.Sent,
          StateMessage = "забрать пассажиров",
          PlaneId = flightInfo.PlaneId,
          FlightId = flightId,
          Point = "C" + flightInfo.PlaneParking + "1", //!!!!!!!
          Details = new
          {
            PassengersCount = boardInfo.NumPassengers,
            takeTo = "XT-1"
          }
        };

        // Сохраняем задачу
        CreateTask(task1);

        // Отправляем в очередь passengerBus
        _rabbitMQService.PublishTask(task1, "tasks.passengerBus");

        // Забрать багаж для baggageTractor
        var task2 = new AirportTask
        {
          TaskId = _currentId++,
          TaskType = "pickUpBaggage",
          State = TaskState.Sent,
          StateMessage = "забрать багаж",
          PlaneId = flightInfo.PlaneId,
          FlightId = flightId,
          Point = "C" + flightInfo.PlaneParking + "2", //!!!!!!!!!!!
          Details = new
          {
            takeTo = "BW-1"
          }
        };

        CreateTask(task2);
        _rabbitMQService.PublishTask(task2, "tasks.baggageTractor");
      }
      catch (Exception ex)
      {
        _consoleLogger.PrintOverlay("In HandleFlightArrivalAsync: " + ex.Message);
        _fileLogger.LogError("In HandleFlightArrivalAsync: " + ex.Message);
      }
    }

    public async Task HandleFlightStatusChangeAsync(string flightId, string status)
    {
      try
      {
        var flightInfo = await _flightInfoService.GetFlightInfoAsync(flightId);
        if (flightInfo == null) throw new Exception("Can't get flight data");
        //var boardInfo = await _boardInfoService.GetBoardInfoAsync(flightInfo.PlaneId);
        //if (boardInfo == null) throw new Exception("Can't get flight data");
        switch (status)
        {
          case "RegistrationClosed":
            // Доставить еду для catering
            var deliverFood = new AirportTask
            {
              TaskId = _currentId++,
              TaskType = "deliverFood",
              State = TaskState.Sent,
              StateMessage = "доставить еду",
              PlaneId = flightInfo.PlaneId,
              FlightId = flightId,
              Point = "C" + flightInfo.PlaneParking + "1",
              Details = new
              {
                takeFrom = "CS-1"
              }
            };
            CreateTask(deliverFood);
            _rabbitMQService.PublishTask(deliverFood, "tasks.catering");

            // Доставить багаж для baggageTractor
            var deliverBaggage = new AirportTask
            {
              TaskId = _currentId++,
              TaskType = "deliverBaggage",
              State = TaskState.Sent,
              StateMessage = "доставить багаж",
              PlaneId = flightInfo.PlaneId,
              FlightId = flightId,
              Point = "C" + flightInfo.PlaneParking + "2",
              Details = new
              {
                takeFrom = GetNumber(flightInfo.Gate)[0]
              }
            };
            CreateTask(deliverBaggage);
            _rabbitMQService.PublishTask(deliverBaggage, "tasks.baggageTractor");

            break;
          case "Boarding":
            var deliverPassengers = new AirportTask
            {
              TaskId = _currentId++,
              TaskType = "deliverPassengers",
              State = TaskState.Sent,
              StateMessage = "доставить пассажиров",
              PlaneId = flightInfo.PlaneId,
              FlightId = flightId,
              Point = "C" + flightInfo.PlaneParking + "1",
              Details = new
              {
                gate = "G-11"
              }
            };

            // Сохраняем задачу
            CreateTask(deliverPassengers);

            // Отправляем в очередь passengerBus
            _rabbitMQService.PublishTask(deliverPassengers, "tasks.passengerBus");
            break;
          case "Departed":
            // Очистка задач
            break;
        }
      }
      catch (Exception ex)
      {
        _consoleLogger.PrintOverlay("In HandleFlightStatusChangeAsync: " + ex.Message);
        _fileLogger.LogError("In HandleFlightStatusChangeAsync: " + ex.Message);
      }
    }

    public async Task HandleRefuelAsync(string planeId, string planeParking, int fuel)
    {
      try
      {
        //var flightInfo = await _flightInfoService.GetFlightInfoAsync(flightId);
        //if (flightInfo == null) throw new Exception("Can't get flight data");
        var boardInfo = await _boardInfoService.GetBoardInfoAsync(planeId);
        if (boardInfo == null) throw new Exception("Can't get flight data");
        // Дозаправить самолет
        var refuel = new AirportTask
        {
          TaskId = _currentId++,
          TaskType = "refuel",
          State = TaskState.Sent,
          StateMessage = "Заправить самолет",
          PlaneId = planeId,
          FlightId = boardInfo.Flight.FlightId,
          Point = "C" + planeParking + "1",
          Details = new
          {
            fuelAmount = fuel
          }
        };
        CreateTask(refuel);
        _rabbitMQService.PublishTask(refuel, "tasks.refueler");
      }
      catch (Exception ex)
      {
        _consoleLogger.PrintOverlay("In HandleFlightStatusChangeAsync: " + ex.Message);
        _fileLogger.LogError("In HandleFlightStatusChangeAsync: " + ex.Message);
      }
    }

    public async Task HandleRefuelTest(string planeId, string planeParking, int fuel)
    {
      try
      {
        //var flightInfo = await _flightInfoService.GetFlightInfoAsync(flightId);
        //if (flightInfo == null) throw new Exception("Can't get flight data");
        //var boardInfo = await _boardInfoService.GetBoardInfoAsync(planeId);
        //if (boardInfo == null) throw new Exception("Can't get flight data");
        // Дозаправить самолет
        _consoleLogger.PrintOverlay($"{planeId}, {planeParking}, {fuel}");
        var refuel = new AirportTask
        {
          TaskId = _currentId++,
          TaskType = "refuel",
          State = TaskState.Sent,
          StateMessage = "Заправить самолет",
          PlaneId = planeId,
          FlightId = "FL123",
          Point = "C" + planeParking + "1",
          Details = new
          {
            fuelAmount = fuel
          }
        };
        CreateTask(refuel);
        _rabbitMQService.PublishTask(refuel, "tasks.refueler");
      }
      catch (Exception ex)
      {
        _consoleLogger.PrintOverlay("In HandleFlightStatusChangeAsync: " + ex.Message);
        _fileLogger.LogError("In HandleFlightStatusChangeAsync: " + ex.Message);
      }
    }

    public async Task HandleFollowMeAsync(string planeId, string Runway, string planeP)
    {
      try
      {
        //var flightInfo = await _flightInfoService.GetFlightInfoAsync(flightId);
        //if (flightInfo == null) throw new Exception("Can't get flight data");
        var boardInfo = await _boardInfoService.GetBoardInfoAsync(planeId);
        if (boardInfo == null) throw new Exception("Can't get flight data");
        // Дозаправить самолет
        var followMe = new AirportTask
        {
          TaskId = _currentId++,
          TaskType = "followMe",
          State = TaskState.Sent,
          StateMessage = "Довести самолет",
          PlaneId = planeId,
          FlightId = boardInfo.Flight.FlightId,
          Point = planeP,
          Details = new
          {
            runway = Runway,
            planeParking = planeP
          }
        };
        CreateTask(followMe);
        _rabbitMQService.PublishTask(followMe, "tasks.followMe");
      }
      catch (Exception ex)
      {
        _consoleLogger.PrintOverlay("In HandleFlightStatusChangeAsync: " + ex.Message);
        _fileLogger.LogError("In HandleFlightStatusChangeAsync: " + ex.Message);
      }
    }

    public AirportTask CreateTask(AirportTask task)
    {
      task.TaskId = _currentId++;
      _tasks[task.TaskId] = task;

      Task.Run(() => _consoleLogger.UpdateTaskLog(task));
      return task;
    }

    public AirportTask? GetTask(int taskId) =>
        _tasks.TryGetValue(taskId, out var task) ? task : null;

    public List<AirportTask> GetAllTasks() => _tasks.Values.ToList();

    public AirportTask? UpdateTask(int taskId, AirportTask task)
    {
      if (!_tasks.ContainsKey(taskId)) return null;
      _tasks[taskId] = task;
      Task.Run(() => _consoleLogger.UpdateTaskLog(task));
      return task;
    }

    public bool AssignTask(int taskId, string carId)
    {
      if (!_tasks.TryGetValue(taskId, out var task)) return false;
      task.CarId = carId;
      task.State = TaskState.Assigned;
      Task.Run(() => _consoleLogger.UpdateTaskLog(task));
      return true;
    }

    public bool CompleteTask(int taskId)
    {
      if (!_tasks.TryGetValue(taskId, out var task)) return false;
      task.State = TaskState.Completed;
      Task.Run(() => _consoleLogger.UpdateTaskLog(task));
      return true;
    }

    private string GetNumber(string node)
    {
      var split = node.Split('-');

      return split[1];
    }
  }
}