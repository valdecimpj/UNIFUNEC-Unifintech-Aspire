using Microsoft.Extensions.Logging;
using Unifintech.Application.Common.Interfaces;
using Unifintech.Domain.Enums;
using Unifintech.Domain.Events;

namespace Unifintech.Application.Loans.EventHandlers;

public class ValidateCreatedLoanCustomerScore : INotificationHandler<LoanCreatedEvent>
{
    private readonly ILogger<ValidateCreatedLoanCustomerScore> _logger;
    private readonly IApplicationDbContext _applicationDbContext;

    public ValidateCreatedLoanCustomerScore(
        ILogger<ValidateCreatedLoanCustomerScore> logger,
        IApplicationDbContext applicationDbContext
    )
    {
        _logger = logger;
        _applicationDbContext = applicationDbContext;
    }

    public async Task Handle(LoanCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("OtherApp Domain Event: {DomainEvent}", notification.GetType().Name);
        var loan = _applicationDbContext.Loans.FirstOrDefault(l => l.Id == notification.Loan.Id);

        if (loan == null)
        {
            _logger.LogWarning("Loan with ID {LoanId} not found.", notification.Loan.Id);
            return;
        }

        // validate the customer score and update the loan status accordingly

        loan.LoanStatus = LoanStatus.Approved;
        await _applicationDbContext.SaveChangesAsync(cancellationToken);
    }
}
