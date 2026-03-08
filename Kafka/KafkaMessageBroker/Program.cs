using KafkaMessageBroker.Kafka;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add Services
builder.Services.AddSingleton<KafkaTopicManager>();
builder.Services.AddSingleton<KafkaProducerService>();
builder.Services.AddControllers();

WebApplication app = builder.Build();

// Add Topic
using (IServiceScope scope = app.Services.CreateScope())
{
    var manager = scope.ServiceProvider.GetRequiredService<KafkaTopicManager>();
    await manager.CreateTopicIfNotExists("test-topic", partitions: 3);
}

app.MapControllers();
app.Run();