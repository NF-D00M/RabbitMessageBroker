using AwsMessageBroker.Aws;
using Microsoft.AspNetCore.Mvc;

namespace AwsMessageBroker.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AwsController : ControllerBase
    {
        private readonly SnsPublisherService _publisher;
        private readonly SnsTopicManager _topicManager;
        private readonly ILogger<AwsController> _logger;

        public AwsController(SnsPublisherService publisher, SnsTopicManager topicManager, ILogger<AwsController> logger)
        {
            _publisher = publisher;
            _topicManager = topicManager;
            _logger = logger;
        }

        [HttpPost("publish/")]
        public async Task<IActionResult> PublishMessage([FromBody] object messageBody)
        {
            try
            {
                // Resolve the Topic ARN (Checks if it exists or creates it)
                string topicArn = await _topicManager.EnsureTopicExistsAsync();

                _logger.LogInformation("Publishing message to SNS Topic: {Arn}", topicArn);

                // Pass message to the Publisher Service
                await _publisher.PublishMessageAsync(topicArn, messageBody);

                return Ok(new
                {
                    Message = "Successfully published to AWS SNS",
                    TopicArn = topicArn,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish to SNS");
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}