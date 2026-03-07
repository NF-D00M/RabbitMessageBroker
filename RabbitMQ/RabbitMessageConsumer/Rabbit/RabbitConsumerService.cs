using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

public class RabbitConsumerService : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly IConnection _connection;
    private IChannel? _channel;

    public RabbitConsumerService(IConfiguration config, IConnection connection)
    {
        _config = config;
        _connection = connection;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Get exchange and queue from config
        string? exchangeName = _config.GetSection("Rabbit:Exchanges:0:Name").Value;
        string? queueName = _config.GetSection("Rabbit:Exchanges:0:Queue:Name").Value;

        if (string.IsNullOrEmpty(exchangeName) || string.IsNullOrEmpty(queueName))
        {
            Console.WriteLine("[ERROR] Configuration missing for Exchange or Queue.");
            return;
        }

        // Check if Rabbit is active 
        if (_connection == null || !_connection.IsOpen)
        {
            Console.WriteLine("[CRITICAL ERROR] RabbitMQ Connection failed: The broker is unreachable.");
            return;
        }

        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        try
        {
            // Check if Exchange exists (passive declaration)
            await _channel.ExchangeDeclarePassiveAsync(exchange: exchangeName, cancellationToken: stoppingToken);
            Console.WriteLine($"[OK] Exchange '{exchangeName}' verified.");

            // Declare Queue 
            // Create queue if queue doesn't exist
            await _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?> { { "x-max-priority", 10 } },
                cancellationToken: stoppingToken
            );

            // 5. Bind Queue to Exchange
            await _channel.QueueBindAsync(
                queue: queueName,
                exchange: exchangeName,
                routingKey: string.Empty,
                cancellationToken: stoppingToken
            );

            Console.WriteLine($"[OK] Queue '{queueName}' bound to '{exchangeName}'.");
        }
        catch (RabbitMQ.Client.Exceptions.OperationInterruptedException)
        {
            Console.WriteLine($"[CRITICAL ERROR] Topology Error: The exchange '{exchangeName}' does not exist. Consumer cannot subscribe.");
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CRITICAL ERROR] Unexpected error during setup: {ex.Message}");
            return;
        }

        // Consumer prefetch (limit number of unacknowledged messages: 1)
        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            byte[] body = ea.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} | " +
                  $"{AppDomain.CurrentDomain.FriendlyName} | " +
                  $"{nameof(ExecuteAsync)} | " +
                  $"Exchange: {exchangeName} | " +
                  $"Queue: {queueName} | " +
                  $"Received: {message} | " +
                  $"Priority: {ea.BasicProperties.Priority}"
            );
            await Task.CompletedTask;
        };

        await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: true,
            consumer: consumer,
            cancellationToken: stoppingToken
        );

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}