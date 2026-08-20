using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Unifintech.Application.Employees.Commands.CreateEmployee;
using Unifintech.Application.Employees.Queries.GetEmployee;

namespace Unifintech.Web.Endpoints;

public class Employees : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateEmployee);
    }

    [EndpointSummary("Create a new employee")]
    [EndpointDescription("Creates a new employee with the specified details.")]
    public static async Task<Results<Created, BadRequest>> CreateEmployee(
        [FromBody] CreateEmployeeCommand command,
        [FromServices] IMediator mediator
    )
    {
        var userId = await mediator.Send(command);
        return TypedResults.Created();
    }

    [EndpointSummary("Get employee by email")]
    [EndpointDescription("Gets an employee by their email address.")]
    public static async Task<Results<Ok<EmployeeVm>, NotFound>> GetEmployeeByEmail(
        [FromQuery] string email,
        [FromServices] IMediator mediator
    )
    {
        var employee = await mediator.Send(new GetEmployeeQuery { Email = email });
        return employee is not null ? TypedResults.Ok(employee) : TypedResults.NotFound();
    }
}
