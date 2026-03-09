using Amazon.SimpleNotificationService;

public class SnsTopicManager
{
    private readonly IAmazonSimpleNotificationService _snsClient;
    private readonly string _topicName;

    public SnsTopicManager(IAmazonSimpleNotificationService snsClient, IConfiguration _config)
    {
        _snsClient = snsClient;
        _topicName = _config["AWS:Topic"];
    }

    public async Task<string> EnsureTopicExistsAsync()
    {
        // Create Topic if not exists, else returns the ARN
        var response = await _snsClient.CreateTopicAsync(_topicName);
        return response.TopicArn;
    }
}