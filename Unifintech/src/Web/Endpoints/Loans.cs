using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Unifintech.Application.Loans.Commands.CreateLoan;
using Unifintech.Application.Loans.Queries.Application;
using Unifintech.Application.Loans.Queries.GetAllLoans;
using Unifintech.Application.Loans.Queries.GetLoan;

namespace Unifintech.Web.Endpoints;

public class Loans : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateLoan);
        groupBuilder.MapGet(GetLoan, "{id}");
        groupBuilder.MapGet(GetAllLoans);
    }

    [EndpointSummary("Create a new loan")]
    [EndpointDescription("Creates a new loan with the specified details.")]
    public static async Task<Results<Created<Guid>, BadRequest>> CreateLoan(
        [FromBody] CreateLoanCommand command,
        [FromServices] IMediator mediator
    )
    {
        var loanId = await mediator.Send(command);
        return TypedResults.Created($"/loans/{loanId}", loanId);
    }

    [EndpointSummary("Get a loan by ID")]
    [EndpointDescription("Retrieves a loan by its unique identifier.")]
    public static async Task<Results<Ok<GetLoanVm>, NotFound>> GetLoan(
        [FromRoute] string id,
        [FromServices] IMediator mediator
    )
    {
        var loan = await mediator.Send(new GetLoanQuery { Id = id });

        if (loan == null)
            return TypedResults.NotFound();

        return TypedResults.Ok(loan);
    }

    [EndpointSummary("Get all loans")]
    [EndpointDescription("Retrieves all loans.")]
    public async static Task<Results<Ok<GetAllLoansVm>, InternalServerError>> GetAllLoans(
        [FromServices] IMediator mediator
    )
    {
        var loans = await mediator.Send(new GetAllLoansQuery());
        return TypedResults.Ok(loans);
    }
}
