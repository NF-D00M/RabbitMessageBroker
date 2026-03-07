using RabbitMQ.Client;
using RabbitMessageBroker.RabbitMQ;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add Rabbit MQ Service
builder.Services.AddSingleton(sp =>
{
    ConnectionFactory factory = new ConnectionFactory 
    { 
        HostName = "127.0.0.1" 
    };

    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

builder.Services.AddSingleton<IMessageBroker, RabbitBroker>();

// Add Controllers
builder.Services.AddControllers();

WebApplication app = builder.Build();

// Initialise Rabbit Exchanges and Queues on Startup
using (IServiceScope scope = app.Services.CreateScope())
{
    IMessageBroker broker = scope.ServiceProvider.GetRequiredService<IMessageBroker>();

    await broker.InitializeDefinitionsAsync();

    Console.WriteLine("RabbitMQ Initialised.");
}

app.MapControllers();
app.Run();
