using Microsoft.AspNetCore.SignalR;

namespace Unifintech.Web.Infrastructure;

public class NameIdentifierUserIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        var userId = connection
            .User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?.Value;
        return userId ?? throw new InvalidOperationException("User is not authenticated.");
    }
}
