using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Unifintech.Application.Common.Interfaces;

namespace Unifintech.Application.Employees.Queries.GetEmployee;

public record GetEmployeeQuery : IRequest<EmployeeVm?>
{
    public string? Email { get; set; }
}

public class GetEmployeeQueryValidator : AbstractValidator<GetEmployeeQuery>
{
    public GetEmployeeQueryValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("A valid Id is required.");
    }
}

public class GetEmployeeQueryHandler : IRequestHandler<GetEmployeeQuery, EmployeeVm?>
{
    private readonly IIdentityService _identityService;
    private readonly IDistributedCache _distributedCache;

    public GetEmployeeQueryHandler(IIdentityService identityService, IDistributedCache distributedCache)
    {
        _identityService = identityService;
        _distributedCache = distributedCache;
    }

    public async Task<EmployeeVm?> Handle(GetEmployeeQuery request, CancellationToken cancellationToken)
    {
        var userFromCache = MaybeDeserializeUser(await _distributedCache.GetStringAsync(request.Email!));

        if (userFromCache is not null)
            return new(userFromCache.Id, userFromCache.Roles);

        var user = await _identityService.GetUserByEmail(request.Email!);

        if (user is null)
            return null;

        await _distributedCache.SetStringAsync(request.Email!, SerializeUser(user), options: new() { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(5)});
        return new (user.Id, user.Roles);
    }

    private string SerializeUser(UserDto employee)
    {
        return JsonSerializer.Serialize(employee);
    }

    private UserDto? MaybeDeserializeUser(string? userJson)
    {
        if (userJson is null)
            return null;
        try
        {
            return JsonSerializer.Deserialize<UserDto>(userJson);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
