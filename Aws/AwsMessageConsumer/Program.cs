using Amazon.SQS;
using Amazon.SimpleNotificationService; 
using AwsMessageConsumer.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure AWS options
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());

// Register queue
builder.Services.AddAWSService<IAmazonSQS>();

// Register topic
builder.Services.AddAWSService<IAmazonSimpleNotificationService>();

// Add services
builder.Services.AddSingleton<SqsQueueManager>();
builder.Services.AddHostedService<SqsConsumerService>();

builder.Services.AddControllers();

WebApplication app = builder.Build(); 

app.MapControllers();
app.Run();