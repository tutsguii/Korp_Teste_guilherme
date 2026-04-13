namespace BuildingBlocks.SharedKernel.Messaging;

public interface IRabbitMqService
{
    void Publish<T>(string routingKey, T message);
}
