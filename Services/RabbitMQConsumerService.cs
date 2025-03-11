using HandlingSupervisor.Models;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace HandlingSupervisor.Services
{
  public class RabbitMQConsumerService : BackgroundService
  {
    private readonly ITaskManager _taskManager;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string QueueName = "flight.status.changed";
    private readonly FileLogger _fileLogger;

    public RabbitMQConsumerService(ITaskManager taskManager, FileLogger fileLogger)
    {
      _taskManager = taskManager;
      var factory = new ConnectionFactory();
      factory.Uri = new Uri("amqp://xnyyznus:OSOOLzaQHT5Ys6NPEMAU5DxTChNu2MUe@hawk.rmq.cloudamqp.com:5672/xnyyznus");
      _connection = factory.CreateConnection();
      _channel = _connection.CreateModel();
      _channel.QueueDeclare(QueueName, durable: false, exclusive: false, autoDelete: false);
      _fileLogger = fileLogger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
      var consumer = new EventingBasicConsumer(_channel);
      consumer.Received += (model, ea) =>
      {
        try
        {
          var body = ea.Body.ToArray();
          var message = Encoding.UTF8.GetString(body);
          var statusMessage = JsonSerializer.Deserialize<FlightStatusMessage>(message);

          _fileLogger.LogInformation("Got status change: " + statusMessage.Status);

          if (statusMessage != null)
          {
            Task.Run(() => ProcessMessage(statusMessage));
          }
        }
        catch (Exception ex)
        {
          _fileLogger.LogError("Error in RabbitMQConsumer: " + ex.Message);
        }
      };

      _channel.BasicConsume(QueueName, autoAck: true, consumer: consumer);
      return Task.CompletedTask;
    }

    private async Task ProcessMessage(FlightStatusMessage message)
    {
      if (message.Status == "Arrived")
      {
        await _taskManager.HandleFlightArrivalAsync(message.FlightId);
      }
      else
      {
        await _taskManager.HandleFlightStatusChangeAsync(message.FlightId, message.Status);
      }
    }

    public override void Dispose()
    {
      _channel.Close();
      _connection.Close();
      base.Dispose();
    }
  }
}