namespace Unifintech.Domain.Entities
{
    public class Loan : BaseAuditableEntity<Guid>
    {
        public Guid CustomerId { get; set; }
        public required string EmployeeId { get; set; }
        public decimal InitialAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int TermInMonths { get; set; }
    }
}
