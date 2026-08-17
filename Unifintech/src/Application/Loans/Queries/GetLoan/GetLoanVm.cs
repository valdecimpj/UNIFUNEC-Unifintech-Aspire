namespace Unifintech.Application.Loans.Queries.GetLoan;

public record GetLoanVm(
    Guid Id,
    string CustomerId,
    string EmployeeId,
    decimal InitialAmount,
    decimal InterestRate,
    int TermInMonths
);
