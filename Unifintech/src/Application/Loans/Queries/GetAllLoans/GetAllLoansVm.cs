using Unifintech.Application.Loans.Queries.GetLoan;

namespace Unifintech.Application.Loans.Queries.GetAllLoans;

public record GetAllLoansVm(IEnumerable<GetLoanVm> Loans);
