using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SocialApp.UserService.Infrastructure.Authentication;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? Id
    {
        get
        {
            var authUserIdValue = _httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(authUserIdValue, out var authUserId)
                ? authUserId
                : null;
        }
    }

    public string? Username => _httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.UniqueName)
        ?? _httpContextAccessor.HttpContext?.User.Identity?.Name;

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;
}
