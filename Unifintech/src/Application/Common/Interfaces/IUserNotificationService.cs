namespace Unifintech.Application.Common.Interfaces;

public interface IUserNotificationService
{
    Task NotifyUserAsync(
        string userId,
        string message,
        CancellationToken cancellationToken = default
    );
}
