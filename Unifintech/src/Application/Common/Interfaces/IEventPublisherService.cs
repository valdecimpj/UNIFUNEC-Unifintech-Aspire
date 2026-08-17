namespace Unifintech.Application.Common.Interfaces;

public interface IEventPublisherService
{
    Task PublishAsync<TEvent>(
        TEvent @event,
        string topicName,
        CancellationToken cancellationToken = default
    );
}
