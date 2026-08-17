using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Unifintech.Application.Common.Interfaces;
using Unifintech.Domain.Common;
using Unifintech.Infrastructure.Extensions;

namespace Unifintech.Infrastructure.Data.Interceptors;

public class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly IEventPublisherService _eventPublisherService;

    public DispatchDomainEventsInterceptor(IEventPublisherService eventPublisherService)
    {
        _eventPublisherService = eventPublisherService;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        DispatchDomainEvents(eventData.Context).GetAwaiter().GetResult();

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        await DispatchDomainEvents(eventData.Context);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public async Task DispatchDomainEvents(DbContext? context)
    {
        if (context == null)
            return;

        var entities = context
            .ChangeTracker.Entries<BaseEntity<Guid>>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity);

        var domainEvents = entities.SelectMany(e => e.DomainEvents).ToList();

        foreach (var domainEvent in domainEvents)
            await _eventPublisherService.PublishAsync(
                domainEvent,
                domainEvent.GetType().Name.ToKebabCase()
            );

        entities.ToList().ForEach(e => e.ClearDomainEvents());
    }
}
