using Unifintech.Application.Common.Interfaces;
using Unifintech.Application.Common.Security;
using Unifintech.Domain.Constants;
using Unifintech.Domain.Events;

namespace Unifintech.Application.Loans.Commands.CreateLoan;

[Authorize(Roles = Roles.Employee + "," + Roles.Administrator)]
public record CreateLoanCommand : IRequest<Guid>
{
    public string? LoanId { get; init; }
    public string? CustomerId { get; init; }
    public string? EmployeeId { get; init; }
    public decimal? InitialAmount { get; init; }
    public decimal? InterestRate { get; init; }
    public int? TermInMonths { get; init; }
}

public class CreateLoanCommandValidator : AbstractValidator<CreateLoanCommand>
{
    public CreateLoanCommandValidator()
    {
        RuleFor(x => x.LoanId).NotEmpty().WithMessage("LoanId is required.");

        RuleFor(x => x.CustomerId).NotEmpty().WithMessage("CustomerId is required.");

        RuleFor(x => x.EmployeeId).NotEmpty().WithMessage("EmployeeId is required.");

        RuleFor(x => x.InitialAmount)
            .GreaterThan(0)
            .WithMessage("InitialAmount must be greater than 0.");

        RuleFor(x => x.InterestRate)
            .GreaterThan(0)
            .WithMessage("InterestRate must be greater than 0.");

        RuleFor(x => x.TermInMonths)
            .GreaterThan(0)
            .WithMessage("TermInMonths must be greater than 0.");
    }
}

public class CreateLoanCommandHandler : IRequestHandler<CreateLoanCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IEventPublisherService _eventPublisherService;

    public CreateLoanCommandHandler(
        IApplicationDbContext context,
        IEventPublisherService eventPublisherService
    )
    {
        _context = context;
        _eventPublisherService = eventPublisherService;
    }

    public async Task<Guid> Handle(CreateLoanCommand request, CancellationToken cancellationToken)
    {
        var existingLoan = await _context.Loans.FirstOrDefaultAsync(
            l => l.Id == Guid.Parse(request.LoanId!),
            cancellationToken
        );

        if (existingLoan != null)
            return existingLoan.Id;

        var loan = new Domain.Entities.Loan
        {
            Id = Guid.Parse(request.LoanId!),
            CustomerId = Guid.Parse(request.CustomerId!),
            EmployeeId = request.EmployeeId!,
            InitialAmount = request.InitialAmount!.Value,
            InterestRate = request.InterestRate!.Value,
            TermInMonths = request.TermInMonths!.Value,
        };

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync(cancellationToken);

        return loan.Id;
    }
}
