using Unifintech.Application.Common.Interfaces;
using Unifintech.Application.Loans.Queries.GetAllLoans;
using Unifintech.Application.Loans.Queries.GetLoan;

namespace Unifintech.Application.Loans.Queries.Application;

public record GetAllLoansQuery : IRequest<GetAllLoansVm>
{
}

public class ApplicationQueryValidator : AbstractValidator<GetAllLoansQuery>
{
    public ApplicationQueryValidator()
    {
    }
}

public class GetAllLoansQueryHandler : IRequestHandler<GetAllLoansQuery, GetAllLoansVm>
{
    private readonly IApplicationDbContext _context;

    public GetAllLoansQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetAllLoansVm> Handle(GetAllLoansQuery request, CancellationToken cancellationToken)
    {
        return new GetAllLoansVm(await _context.Loans.Select(loan => new GetLoanVm(
            loan.Id,
            loan.CustomerId,
            loan.EmployeeId,
            loan.InitialAmount,
            loan.InterestRate,
            loan.TermInMonths,
            loan.LoanStatus,
            null
        )).ToListAsync(cancellationToken));
    }
}
