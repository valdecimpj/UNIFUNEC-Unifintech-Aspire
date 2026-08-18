using Microsoft.Extensions.Logging;
using Unifintech.Application.Common.Interfaces;
using Unifintech.Domain.Enums;
using Unifintech.Domain.Events;

namespace Unifintech.Application.Loans.EventHandlers;

public class ValidateCreatedLoanCustomerScore : INotificationHandler<LoanCreatedEvent>
{
    private readonly ILogger<ValidateCreatedLoanCustomerScore> _logger;
    private readonly IApplicationDbContext _applicationDbContext;
    private readonly ICustomerCreditService _customerCreditService;

    public ValidateCreatedLoanCustomerScore(
        ILogger<ValidateCreatedLoanCustomerScore> logger,
        IApplicationDbContext applicationDbContext,
        ICustomerCreditService customerCreditService
    )
    {
        _logger = logger;
        _applicationDbContext = applicationDbContext;
        _customerCreditService = customerCreditService;
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

        var customerScore = await _customerCreditService.GetCustomerCreditScoreAsync(
            loan.CustomerId.ToString(),
            cancellationToken
        );

        if (customerScore == null)
            throw new Exception($"Customer score not found for customer ID {loan.CustomerId}");

        if (customerScore < 600)
        {
            loan.LoanStatus = LoanStatus.Rejected;
            await _applicationDbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        loan.LoanStatus = LoanStatus.Approved;
        await _applicationDbContext.SaveChangesAsync(cancellationToken);
    }
}
