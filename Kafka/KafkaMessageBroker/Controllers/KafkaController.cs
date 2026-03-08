using KafkaMessageBroker.Kafka;
using Microsoft.AspNetCore.Mvc;

namespace KafkaMessageBroker.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class KafkaController : ControllerBase
    {
        private readonly KafkaProducerService _producer;

        public KafkaController(KafkaProducerService producer) => _producer = producer;

        [HttpPost("publish/topic/{topic}")]
        public async Task<IActionResult> Post(string topic, [FromBody] dynamic message, [FromQuery] string? key)
        {
            var result = await _producer.PublishAsync(topic, key ?? Guid.NewGuid().ToString(), message);
            return Ok(new { result.Topic, result.Partition, result.Offset });
        }
    }
}
