using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Unifintech.Application.Common.Interfaces;

namespace Unifintech.Infrastructure.Pub;

public class KafkaEventPublisherService : IEventPublisherService
{
    private readonly ProducerConfig _producerConfig;

    public KafkaEventPublisherService(IConfiguration configuration)
    {
        var kafkaConnectionString =
            configuration.GetConnectionString("kafka")
            ?? throw new ArgumentNullException("Kafka connection string is not configured.");

        _producerConfig = new ProducerConfig
        {
            BootstrapServers = kafkaConnectionString,
            AllowAutoCreateTopics = true,
            Acks = Acks.All,
            EnableIdempotence = true,
        };
    }

    public async Task PublishAsync<TEvent>(
        TEvent @event,
        string topicName,
        CancellationToken cancellationToken = default
    )
    {
        using var producer = new ProducerBuilder<Null, string>(_producerConfig).Build();

        if (@event == null)
            return;

        var message = new Message<Null, string>
        {
            Value = System.Text.Json.JsonSerializer.Serialize(@event, @event.GetType()),
        };

        await producer.ProduceAsync(topicName, message, cancellationToken);
    }
}
