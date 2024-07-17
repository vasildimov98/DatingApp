using System.Security.Claims;

namespace API.Extensions;

public static class ClaimsPrincipleExtensions
{
    public static string GetUsername(this ClaimsPrincipal claimsPrincipal)
    {
        var username = claimsPrincipal
            .FindFirstValue(ClaimTypes.NameIdentifier)
             ?? throw new Exception("Cannot get username from token");

        return username;
    }
}
