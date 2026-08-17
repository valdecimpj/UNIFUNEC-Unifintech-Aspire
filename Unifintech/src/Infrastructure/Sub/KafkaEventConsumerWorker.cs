using System.Text.Json;
using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Unifintech.Infrastructure.Sub;

public class KafkaEventConsumerWorker<TEvent> : BackgroundService
    where TEvent : INotification
{
    private readonly ConsumerConfig _consumerConfig;
    private readonly IServiceScope _serviceScope;
    private readonly ILogger<KafkaEventConsumerWorker<TEvent>> _logger;
    private readonly string _topic;

    public KafkaEventConsumerWorker(IServiceScope serviceScope, string topic)
    {
        _topic = topic;
        _serviceScope = serviceScope;
        var configuration = _serviceScope.ServiceProvider.GetRequiredService<IConfiguration>();
        var kafkaConnection = configuration.GetConnectionString("kafka");

        _logger = _serviceScope.ServiceProvider.GetRequiredService<
            ILogger<KafkaEventConsumerWorker<TEvent>>
        >();

        _consumerConfig = new()
        {
            BootstrapServers = kafkaConnection,
            Acks = Acks.All,
            EnableAutoCommit = false,
            GroupId = $"kafka-event-consumer-{typeof(TEvent).Name}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
        consumer.Subscribe(_topic);

        _logger.LogInformation("Kafka consumer started for topic: {Topic}", _topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);

                    if (consumeResult?.Message?.Value == null)
                        continue;

                    var kafkaEvent = JsonSerializer.Deserialize<TEvent>(
                        consumeResult.Message.Value
                    );

                    if (kafkaEvent != null)
                    {
                        using var scope = _serviceScope.ServiceProvider.CreateScope();
                        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

                        await publisher.Publish(kafkaEvent, stoppingToken);

                        consumer.Commit(consumeResult);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing Kafka message on topic {Topic}", _topic);
                }
            }
        }
        finally
        {
            consumer.Close();
        }
    }

    public override void Dispose()
    {
        _serviceScope.Dispose();
        base.Dispose();
    }
}
