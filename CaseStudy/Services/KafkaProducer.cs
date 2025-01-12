using Confluent.Kafka;

namespace CaseStudy.Services
{
    // BONUS: Pro přijetí informace o zaplacení objednávky zajistěte asynchroní zpracování plateb pomocí fronty
    public class KafkaProducer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<KafkaProducer> _logger;
        private readonly IProducer<Null, string> _producer;

        public KafkaProducer(IConfiguration configuration, ILogger<KafkaProducer> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var producerconfig = new ProducerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"]
            };

            _producer = new ProducerBuilder<Null, string>(producerconfig).Build();
        }

        // Uložení message do Kafky
        public async Task ProduceAsync(string topic, string message)
        {
            var kafkamessage = new Message<Null, string> { Value = message, };

            try
            {
                await _producer.ProduceAsync(topic, kafkamessage);
            }
            catch (ProduceException<Null, string> ex)
            {
                _logger.LogError($"Error producing message: {ex.Error.Reason}");
            }
        }
    }
}
