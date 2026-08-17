using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Unifintech.Application.Employees.Commands.CreateEmployee;

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
}
