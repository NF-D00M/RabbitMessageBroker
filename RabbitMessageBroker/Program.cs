using RabbitMessageBroker.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRabbitMQ("127.0.0.1");
builder.Services.AddHostedService<RabbitCleanupService>();
builder.Services.AddHostedService<RabbitConsumerService>();

builder.Services.AddControllers();

WebApplication app = builder.Build();

app.MapControllers(); 

app.Run();