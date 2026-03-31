using System.Security.Claims;

namespace Bobail.API.Extensions;

public static class ClaimsExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (id == null)
            throw new Exception("UserId not found in token");

        return Guid.Parse(id);
    }
}