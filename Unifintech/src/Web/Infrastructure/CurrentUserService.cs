using Unifintech.Application.Common.Interfaces;

namespace Unifintech.Web.Infrastructure;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetCurrentUserId()
    {
        var userId = _httpContextAccessor
            .HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?.Value;
        return userId ?? throw new InvalidOperationException("User is not authenticated.");
    }
}
