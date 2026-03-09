using KafkaMessageBroker.Kafka;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add Services
builder.Services.AddSingleton<KafkaTopicManager>();
builder.Services.AddSingleton<KafkaProducerService>();
builder.Services.AddControllers();

WebApplication app = builder.Build();

IConfiguration _config = app.Configuration;

// Add Topic
using (IServiceScope scope = app.Services.CreateScope())
{
    var manager = scope.ServiceProvider.GetRequiredService<KafkaTopicManager>();
    await manager.CreateTopicIfNotExists(_config.GetValue<string>("Kafka:Topic"), partitions: 3);
}

app.MapControllers();
app.Run();