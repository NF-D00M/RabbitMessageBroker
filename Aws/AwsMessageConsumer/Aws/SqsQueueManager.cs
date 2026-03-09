using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
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
        // Get ARNs for topic and queue
        CreateTopicResponse topicResponse = await _snsClient.CreateTopicAsync(_topicName);
        string topicArn = topicResponse.TopicArn;

        GetQueueAttributesResponse attributes = await _sqsClient.GetQueueAttributesAsync(queueUrl, new List<string> { "QueueArn" });
        string queueArn = attributes.QueueARN;

        // Subscribe
        await _snsClient.SubscribeAsync(topicArn, "sqs", queueArn);

        // Apply policy
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

        SetQueueAttributesRequest attrRequest = new SetQueueAttributesRequest
        {
            QueueUrl = queueUrl,
            Attributes = new Dictionary<string, string> { { "Policy", policy } }
        };

        await _sqsClient.SetQueueAttributesAsync(attrRequest);
    }
}