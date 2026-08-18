using Unifintech.Application.Common.Interfaces;
using Unifintech.Application.Common.Security;
using Unifintech.Domain.Constants;

namespace Unifintech.Application.Loans.Queries.GetLoan;

[Authorize(Roles = Roles.Employee + "," + Roles.Administrator)]
public record GetLoanQuery : IRequest<GetLoanVm?>
{
    public string? Id { get; init; }
}

public class GetLoanQueryValidator : AbstractValidator<GetLoanQuery>
{
    public GetLoanQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Loan Id is required.")
            .Must(id => Guid.TryParse(id, out _))
            .WithMessage("Loan Id must be a valid GUID.");
    }
}

public class GetLoanQueryHandler : IRequestHandler<GetLoanQuery, GetLoanVm?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICustomerCreditService _customerCreditService;

    public GetLoanQueryHandler(
        IApplicationDbContext context,
        ICustomerCreditService customerCreditService
    )
    {
        _context = context;
        _customerCreditService = customerCreditService;
    }

    public async Task<GetLoanVm?> Handle(GetLoanQuery request, CancellationToken cancellationToken)
    {
        var loan = await _context
            .Loans.Where(l => l.Id == Guid.Parse(request.Id!))
            .FirstOrDefaultAsync(cancellationToken);

        if (loan == null)
            return null;

        var currentCustomerCreditScore = await _customerCreditService.GetCustomerCreditScoreAsync(
            loan.CustomerId,
            cancellationToken
        );

        return new GetLoanVm(
            loan.Id,
            loan.CustomerId,
            loan.EmployeeId,
            loan.InitialAmount,
            loan.InterestRate,
            loan.TermInMonths,
            loan.LoanStatus,
            currentCustomerCreditScore
        );
    }
}
