using Unifintech.Application.Common.Interfaces;
using Unifintech.Domain.Events;

namespace Unifintech.Application.Employees.EventHandlers;

public class NotifyFiredEmployee : INotificationHandler<EmployeeFiredEvent>
{
    private readonly IUserNotificationService _userNotificationService;

    public NotifyFiredEmployee(IUserNotificationService userNotificationService)
    {
        _userNotificationService = userNotificationService;
    }

    public async Task Handle(EmployeeFiredEvent notification, CancellationToken cancellationToken)
    {
        await _userNotificationService.NotifyUserAsync(
            notification.EmployeeId,
            "You have been terminated from your position. Please contact HR for further details."
        );
    }
}
