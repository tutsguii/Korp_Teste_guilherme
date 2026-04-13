using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace BuildingBlocks.SharedKernel.Messaging;

public class RabbitMqService : IRabbitMqService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string ExchangeName = "korp.eventos";

    public RabbitMqService(string hostName, string userName, string password)
    {
        var factory = new ConnectionFactory
        {
            HostName = hostName,
            UserName = userName,
            Password = password
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: ExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false);
    }

    public void Publish<T>(string routingKey, T message)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;

        _channel.BasicPublish(
            exchange: ExchangeName,
            routingKey: routingKey,
            basicProperties: properties,
            body: body);
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}
