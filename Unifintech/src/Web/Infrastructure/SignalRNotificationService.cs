using System;
using Microsoft.AspNetCore.SignalR;
using Unifintech.Application.Common.Interfaces;
using Unifintech.Web.Hubs;

namespace Unifintech.Web.Infrastructure;

public class SignalRNotificationService : IUserNotificationService
{
    private readonly IHubContext<EventHub> _hubContext;

    public SignalRNotificationService(IHubContext<EventHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyUserAsync(
        string userId,
        string message,
        CancellationToken cancellationToken = default
    )
    {
        return _hubContext
            .Clients.User(userId)
            .SendAsync("ReceiveNotification", message, cancellationToken);
    }
}
