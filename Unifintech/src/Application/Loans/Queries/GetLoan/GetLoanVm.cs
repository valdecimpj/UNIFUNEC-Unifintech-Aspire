using Unifintech.Domain.Enums;

namespace Unifintech.Application.Loans.Queries.GetLoan;

public record GetLoanVm(
    Guid Id,
    string CustomerId,
    string EmployeeId,
    decimal InitialAmount,
    decimal InterestRate,
    int TermInMonths,
    LoanStatus LoanStatus,
    decimal? CurrentCustomerCreditScore
);
