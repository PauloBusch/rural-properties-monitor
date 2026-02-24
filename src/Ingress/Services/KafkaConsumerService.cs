using Confluent.Kafka;
using IngressApi.Models;
using Sensors.Models;
using System.Text.Json;
using IngressApi.Repositories;

namespace IngressApi.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly KafkaConfig _kafkaConfig;
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly ISensorDataRepository _repository;

        public KafkaConsumerService(
            KafkaConfig kafkaConfig,
            ILogger<KafkaConsumerService> logger,
            ISensorDataRepository repository)
        {
            _kafkaConfig = kafkaConfig;
            _logger = logger;
            _repository = repository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Yield para não bloquear a inicialização da aplicação (Swagger, controllers, etc.)
            await Task.Yield();

            var conf = new ConsumerConfig
            {
                BootstrapServers = _kafkaConfig.Broker,
                GroupId = _kafkaConfig.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Connecting to Kafka broker at {Broker}...", _kafkaConfig.Broker);

                    using var consumer = new ConsumerBuilder<Ignore, string>(conf).Build();
                    consumer.Subscribe(_kafkaConfig.Topic);

                    _logger.LogInformation("Connected to Kafka. Consuming topic '{Topic}'...", _kafkaConfig.Topic);

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            var result = consumer.Consume(stoppingToken);
                            var data = JsonSerializer.Deserialize<SensorDataPayload>(result.Message.Value);
                            _logger.LogInformation("Received message: {Message}", result.Message.Value);
                            _logger.LogInformation("Deserialized data: {@Data}", data);

                            if (data != null)
                            {
                                await _repository.SaveAsync(data, stoppingToken);
                                _logger.LogInformation("Data saved to InfluxDB.");
                            }
                        }
                        catch (ConsumeException ex)
                        {
                            _logger.LogError(ex, "Kafka consume error: {Reason}", ex.Error.Reason);
                        }
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // Shutdown gracioso — não logar como erro
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kafka connection failed. Retrying in 10 seconds...");
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
        }
    }
}