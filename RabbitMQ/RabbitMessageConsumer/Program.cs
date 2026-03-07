using RabbitMQ.Client;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(sp =>
{
    var factory = new ConnectionFactory { HostName = "127.0.0.1" };
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

builder.Services.AddHostedService<RabbitConsumerService>();

builder.Services.AddControllers();

WebApplication app = builder.Build();

app.MapControllers();

app.Run();
