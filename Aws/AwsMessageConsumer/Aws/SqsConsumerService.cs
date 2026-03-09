using Amazon.SQS;
using Amazon.SQS.Model;

namespace AwsMessageConsumer.Services
{
    public class SqsConsumerService : BackgroundService
    {
        private readonly ILogger<SqsConsumerService> _logger;
        private readonly IAmazonSQS _sqsClient;
        private readonly SqsQueueManager _queueManager;

        public SqsConsumerService( ILogger<SqsConsumerService> logger, IAmazonSQS sqsClient, SqsQueueManager queueManager)
        {
            _logger = logger;
            _sqsClient = sqsClient;
            _queueManager = queueManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Create queue if not exist
            string queueUrl = await _queueManager.EnsureQueueExistsAsync();
            await _queueManager.SubscribeAndAllowSnsAsync(queueUrl);

            _logger.LogInformation("Connected to SQS: {Url}", queueUrl);

            // Poll
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var request = new ReceiveMessageRequest
                    {
                        QueueUrl = queueUrl,
                        WaitTimeSeconds = 20, // Long polling 
                        MaxNumberOfMessages = 10
                    };

                    ReceiveMessageResponse response = await _sqsClient.ReceiveMessageAsync(request, stoppingToken);

                    if (response.Messages.Count > 0)
                    {
                        foreach (var message in response.Messages)
                        {
                            _logger.LogInformation("\n[MESSAGE RECEIVED]\nID: {Id}\nBody:\n{Body}",
                                message.MessageId,
                                FormatJson(message.Body)
                            );

                            // Delete message once consumed or else it will reappear
                            await _sqsClient.DeleteMessageAsync(queueUrl, message.ReceiptHandle, stoppingToken);

                            _logger.LogInformation("Message deleted/acknowledged.");
                        }
                    }
                }
                catch (OperationCanceledException) 
                {
                    
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while polling SQS.");
                    await Task.Delay(5000, stoppingToken); 
                }
            }
        }

        private string FormatJson(string json)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                return System.Text.Json.JsonSerializer.Serialize(doc, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            }
            catch
            {
                return json; 
            }
        }
    }
}