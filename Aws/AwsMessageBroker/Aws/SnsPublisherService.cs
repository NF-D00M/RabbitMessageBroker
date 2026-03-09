using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System.Text.Json;

namespace AwsMessageBroker.Aws
{ 

    public class SnsPublisherService
    {
        private readonly IAmazonSimpleNotificationService _snsClient;

        public SnsPublisherService(IAmazonSimpleNotificationService snsClient) => _snsClient = snsClient;

        public async Task PublishMessageAsync(string topicArn, object message)
        {
            PublishRequest request = new PublishRequest
            {
                TopicArn = topicArn,
                Message = JsonSerializer.Serialize(message)
            };
            await _snsClient.PublishAsync(request);
        }
    }

}
