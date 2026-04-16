using System.Security.Claims;

namespace DashHubApi.Application.Common;

public static class ClaimsPrincipalExtensions
{
    public static int ObterIdUsuario(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            throw new ExcecaoApi("Token inválido", 401);
        }

        return userId;
    }
}
