﻿using CaseStudy.Models;
using Confluent.Kafka;

namespace CaseStudy.Services
{    
    // BONUS: Pro přijetí informace o zaplacení objednávky zajistěte asynchroní zpracování plateb pomocí fronty
    // Osobně nemám s asynchonním zpracováním pomocí fronty zkušenosti, nicméně v aktuálním zaměstnání je k podobným problémům využita Kafka, tak jsem zvolila právě tu
    public class KafkaConsumer : BackgroundService
    {
        private readonly ConsumerConfig _consumerConfig;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<KafkaConsumer> _logger;

        public KafkaConsumer(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, ILogger<KafkaConsumer> logger)
        {
            _configuration = configuration;
            _logger = logger;

            _consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"],
                GroupId = "order-payment-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };

            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                //Přijetí message uložené do Kafky
                using var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
                consumer.Subscribe(_configuration["Kafka:PaymentTopic"]);

                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            var consumeResult = consumer.Consume(stoppingToken);
                            if (consumeResult != null)
                            {
                                await ProcessMessageAsync(consumeResult.Message.Value);
                            }
                        }
                        catch (ConsumeException ex)
                        {
                            _logger.LogError($"Kafka consume error: {ex.Message}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Kafka consumer operation canceled.");
                }
                finally
                {
                    consumer.Close();
                }

            }, stoppingToken);
        }

        // Zpracování message uložené do Kafky
        private async Task ProcessMessageAsync(string message)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                var messageBody = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageInput>(message);

                if (messageBody != null)
                {
                    await orderService.MarkOrderAsPaid(messageBody);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing message: {ex.Message}");
            }
        }
    }
}
