namespace Unifintech.Domain.Events;

public class LoanCreatedEvent : BaseEvent
{
    public required Loan Loan { get; init; }
}
