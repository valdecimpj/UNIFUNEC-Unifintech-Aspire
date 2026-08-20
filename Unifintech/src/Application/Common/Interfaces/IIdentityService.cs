using Unifintech.Application.Common.Models;

namespace Unifintech.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);
    Task<UserDto?> GetUserByEmail(string email);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);

    Task<Result> DeleteUserAsync(string userId);

    Task<Result> AddUserToRoleAsync(string userId, string role);
    Task<IList<string>> GetUserRolesAsync(string userId);
}

public record UserDto(string Id, string email, IEnumerable<string> Roles);
