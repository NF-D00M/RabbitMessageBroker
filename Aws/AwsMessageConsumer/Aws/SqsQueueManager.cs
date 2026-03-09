using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Amazon.SQS.Model;

public class SqsQueueManager
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IAmazonSimpleNotificationService _snsClient;
    private readonly string _queueName;
    private readonly string _topicName;

    public SqsQueueManager(IAmazonSQS sqsClient, IAmazonSimpleNotificationService snsClient, IConfiguration config)
    {
        _sqsClient = sqsClient;
        _snsClient = snsClient;
        _queueName = config["AWS:QueueName"] ?? "aws-topic-sqs-consumer";
        _topicName = config["AWS:TopicName"] ?? "aws-topic";
    }

    public async Task<string> EnsureQueueExistsAsync()
    {
        try
        {
            var response = await _sqsClient.GetQueueUrlAsync(_queueName);
            return response.QueueUrl;
        }
        catch (QueueDoesNotExistException)
        {
            var createResponse = await _sqsClient.CreateQueueAsync(_queueName);
            return createResponse.QueueUrl;
        }
    }

    public async Task SubscribeAndAllowSnsAsync(string queueUrl)
    {
        // 1. Get ARNs for both resources
        var attributes = await _sqsClient.GetQueueAttributesAsync(queueUrl, new List<string> { "QueueArn" });
        var queueArn = attributes.QueueARN;

        var topicResponse = await _snsClient.CreateTopicAsync(_topicName);
        var topicArn = topicResponse.TopicArn;

        // 2. Create the Subscription
        await _snsClient.SubscribeAsync(topicArn, "sqs", queueArn);

        // 3. Create and Apply the Policy (The "Door Unlock")
        var policy = $@"{{
            ""Version"": ""2012-10-17"",
            ""Statement"": [{{
                ""Effect"": ""Allow"",
                ""Principal"": {{ ""Service"": ""sns.amazonaws.com"" }},
                ""Action"": ""sqs:SendMessage"",
                ""Resource"": ""{queueArn}"",
                ""Condition"": {{
                    ""ArnEquals"": {{ ""aws:SourceArn"": ""{topicArn}"" }}
                }}
            }}]
        }}";

        var attrRequest = new SetQueueAttributesRequest
        {
            QueueUrl = queueUrl,
            Attributes = new Dictionary<string, string> { { "Policy", policy } }
        };

        await _sqsClient.SetQueueAttributesAsync(attrRequest);
    }
}