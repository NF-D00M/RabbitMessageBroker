using RabbitMessageBroker.Models;
using RabbitMessageBroker.RabbitMQ;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

public class RabbitBroker : IMessageBroker
{
    private readonly IConfiguration _config;
    private readonly IConnection _connection;

    public RabbitBroker(IConfiguration config, IConnection connection)
    {
        _config = config;
        _connection = connection;
    }

    public async Task PublishAsync<T>(string exchangeName, byte priority, T message)
    {
        using IChannel channel = await _connection.CreateChannelAsync();

        // Ensure exchange exists before publishing
        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false
        );

        byte[] body = message is string str
            ? Encoding.UTF8.GetBytes(str)
            : JsonSerializer.SerializeToUtf8Bytes(message);

        var properties = new BasicProperties
        {
            Priority = priority,
            Persistent = true
        };

        // Publish to the exchange (if no queues are bound the message is dropped)
        await channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: string.Empty,
            mandatory: false,
            basicProperties: properties,
            body: body
        );
    }

    public async Task InitializeDefinitionsAsync()
    {
        using IChannel channel = await _connection.CreateChannelAsync();

        List<Exchange> exchanges = _config.GetSection("Rabbit:Exchanges").Get<List<Exchange>>();

        if (exchanges == null)
        {
            return;
        }

        foreach (Exchange exchange in exchanges)
        {
            // Declare exchange
            await channel.ExchangeDeclareAsync(
                exchange: exchange.Name,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false
            );
        }
    }
}