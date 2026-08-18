using Unifintech.Application.Common.Interfaces;
using Unifintech.Domain.Constants;
using Unifintech.Domain.Events;

namespace Unifintech.Application.Employees.EventHandlers;

public class FireEmployeeForLowInterestCreatedLoan : INotificationHandler<LoanCreatedEvent>
{
    private readonly IIdentityService _identityService;
    private readonly IEventPublisherService _eventPublisherService;

    public FireEmployeeForLowInterestCreatedLoan(
        IIdentityService identityService,
        IEventPublisherService eventPublisherService
    )
    {
        _identityService = identityService;
        _eventPublisherService = eventPublisherService;
    }

    public async Task Handle(LoanCreatedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.Loan.InterestRate > 25)
            return;

        var roles = await _identityService.GetUserRolesAsync(notification.Loan.EmployeeId);

        if (roles.Contains(Roles.Administrator))
            return;

        await _identityService.DeleteUserAsync(notification.Loan.EmployeeId);

        await _eventPublisherService.PublishAsync(
            new EmployeeFiredEvent
            {
                EmployeeId = notification.Loan.EmployeeId,
                Reason = "Low interest rate on created loan.",
            }
        );
    }
}
