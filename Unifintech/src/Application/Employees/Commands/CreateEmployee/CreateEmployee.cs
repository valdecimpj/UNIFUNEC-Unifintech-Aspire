using Unifintech.Application.Common.Interfaces;
using Unifintech.Application.Common.Security;
using Unifintech.Domain.Constants;

namespace Unifintech.Application.Employees.Commands.CreateEmployee;

[Authorize(Roles = Roles.Administrator)]
public record CreateEmployeeCommand : IRequest<string>
{
    public string? Email { get; init; }
    public string? Password { get; init; }
}

public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters long.");
    }
}

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, string>
{
    private readonly IIdentityService _identityService;

    public CreateEmployeeCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<string> Handle(
        CreateEmployeeCommand request,
        CancellationToken cancellationToken
    )
    {
        var (result, userId) = await _identityService.CreateUserAsync(
            request.Email!,
            request.Password!
        );

        if (!result.Succeeded)
            throw new ValidationException(
                result.Errors.Select(e => new FluentValidation.Results.ValidationFailure(
                    "Password",
                    e
                ))
            );

        await _identityService.AddUserToRoleAsync(userId, Roles.Employee);

        return userId;
    }
}
