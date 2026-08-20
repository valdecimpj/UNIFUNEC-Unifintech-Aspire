namespace Unifintech.Application.Employees.Queries.GetEmployee;

public record EmployeeVm(string email, IEnumerable<string> roles);
