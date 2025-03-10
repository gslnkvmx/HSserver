using HandlingSupervisor.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace HandlingSupervisor.Services
{
  public class RabbitMQService
  {
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQService()
    {
      var factory = new ConnectionFactory();
      factory.Uri = new Uri("amqp://xnyyznus:OSOOLzaQHT5Ys6NPEMAU5DxTChNu2MUe@hawk.rmq.cloudamqp.com:5672/xnyyznus");
      _connection = factory.CreateConnection();
      _channel = _connection.CreateModel();
    }

    public void PublishTask(AirportTask task, string queueName)
    {
      _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false);

      var json = JsonSerializer.Serialize(task);
      var body = Encoding.UTF8.GetBytes(json);

      _channel.BasicPublish(
          exchange: "",
          routingKey: queueName,
          basicProperties: null,
          body: body);
    }

    public void Dispose()
    {
      _channel.Close();
      _connection.Close();
    }
  }
}