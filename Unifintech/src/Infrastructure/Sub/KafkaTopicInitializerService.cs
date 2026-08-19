using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Unifintech.Infrastructure.Sub;

public class KafkaTopicInitializerService
{
    private readonly AdminClientConfig _adminConfig;
    private readonly ILogger<KafkaTopicInitializerService> _logger;

    public KafkaTopicInitializerService(IConfiguration configuration, ILogger<KafkaTopicInitializerService> logger)
    {
        _logger = logger;
        var kafkaConnection = configuration.GetConnectionString("kafka");
        _adminConfig = new AdminClientConfig
        {
            BootstrapServers = kafkaConnection
        };
    }

    public async Task EnsureTopicExistsAsync(string topicName, int partitions = 3, short replicationFactor = 1)
    {
        using var adminClient = new AdminClientBuilder(_adminConfig).Build();

        try
        {
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));
            bool topicExists = metadata.Topics.Any(t => t.Topic == topicName);

            if (topicExists)
            {
                _logger.LogInformation("Kafka topic '{Topic}' already exists.", topicName);
                return;
            }

            _logger.LogInformation("Kafka topic '{Topic}' not found. Creating...", topicName);
            var topicSpecification = new TopicSpecification
            {
                Name = topicName,
                NumPartitions = partitions,
                ReplicationFactor = replicationFactor
            };

            await adminClient.CreateTopicsAsync(new[] { topicSpecification });
            _logger.LogInformation("Kafka topic '{Topic}' successfully created.", topicName);
        }
        catch (CreateTopicsException e) when (e.Results[0].Error.Code == ErrorCode.TopicAlreadyExists)
        {
            _logger.LogInformation("Kafka topic '{Topic}' was already created concurrently.", topicName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to guarantee existence of Kafka topic '{Topic}'.", topicName);
            throw;
        }
    }
}
