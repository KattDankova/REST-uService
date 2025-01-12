using CaseStudy.Models;
using Confluent.Kafka;

namespace CaseStudy.Services
{
    public class Message
    {
        public string IDOrder { get; set; }
        public bool Paid { get; set; }
    }

    public class KafkaOrderPaymentConsumer : BackgroundService
    {
        private readonly ConsumerConfig _consumerConfig;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public KafkaOrderPaymentConsumer(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        {
            // Configure Kafka consumer
            _consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"],
                GroupId = "order-payment-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };

            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                using var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
                consumer.Subscribe("Payment_topic");

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(stoppingToken);
                        if (consumeResult != null)
                        {
                            Console.WriteLine($"Message received: {consumeResult.Message.Value}");
                            await ProcessMessageAsync(consumeResult.Message.Value);
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        Console.WriteLine($"Kafka consume error: {ex.Message}");
                    }
                }

                consumer.Close();
            }, stoppingToken);
        }


        private async Task ProcessMessageAsync(string message)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();

                // Resolve the scoped service
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                // Parse the message (e.g., JSON to an object)
                var messageBody = Newtonsoft.Json.JsonConvert.DeserializeObject<Message>(message);

                // Update the order in the database
                if (messageBody != null)
                {
                    await orderService.MarkOrderAsPaid(messageBody.IDOrder, messageBody.Paid);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }
        }
    }
}
