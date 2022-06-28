using System.Security.Claims;
using OpenIddict.Abstractions;
using Promos.Authorization.Data;

namespace Promos.Authorization.Helpers;

public static class PrincipalExtensions
{
    public static UserRequest ToUserRequest(this ClaimsPrincipal? principal)
    {
        if (principal == null)
            throw new InvalidOperationException("The user details cannot be retrieved.");

        var userName = principal.FindFirstValue(OpenIddictConstants.Claims.Username);
        if (string.IsNullOrWhiteSpace(userName))
            throw new InvalidOperationException("The user details cannot be retrieved.");

        return new UserRequest(userName);
    }
}