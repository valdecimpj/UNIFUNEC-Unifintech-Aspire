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

    public GetLoanQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetLoanVm?> Handle(GetLoanQuery request, CancellationToken cancellationToken)
    {
        var loan = await _context
            .Loans.Where(l => l.Id == Guid.Parse(request.Id!))
            .Select(l => new GetLoanVm(
                l.Id,
                l.CustomerId.ToString(),
                l.EmployeeId,
                l.InitialAmount,
                l.InterestRate,
                l.TermInMonths
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return loan;
    }
}
