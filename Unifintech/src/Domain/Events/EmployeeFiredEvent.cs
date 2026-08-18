namespace Unifintech.Domain.Events;

public class EmployeeFiredEvent : BaseEvent
{
    public required string EmployeeId { get; init; }
    public required string Reason { get; init; }
}
