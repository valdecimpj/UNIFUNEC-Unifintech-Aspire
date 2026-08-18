namespace Unifintech.Domain.Entities
{
    public class Loan : BaseAuditableEntity<Guid>
    {
        public string CustomerId { get; set; }
        public required string EmployeeId { get; set; }
        public decimal InitialAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int TermInMonths { get; set; }
        public LoanStatus LoanStatus { get; set; }

        public Loan()
        {
            LoanStatus = LoanStatus.Pending;
        }
    }
}
