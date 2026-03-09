using Amazon.SimpleNotificationService;
using AwsMessageBroker.Aws;

var builder = WebApplication.CreateBuilder(args);

// Configure AWS options and register SNS client
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonSimpleNotificationService>();

// Add services
builder.Services.AddSingleton<SnsTopicManager>();
builder.Services.AddSingleton<SnsPublisherService>();

builder.Services.AddControllers();

var app = builder.Build();

// Create Topic if not exist
using (var scope = app.Services.CreateScope())
{
    ILogger logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    SnsTopicManager topicManager = scope.ServiceProvider.GetRequiredService<SnsTopicManager>();

    try
    {
        logger.LogInformation("Verifying AWS SNS Infrastructure...");
        string topicArn = await topicManager.EnsureTopicExistsAsync();
        logger.LogInformation($"SNS Topic is ready. ARN: {topicArn}");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Failed to connect to AWS. Check your credentials/region.");
    }
}

app.MapControllers();
app.Run();