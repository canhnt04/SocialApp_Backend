using System.Security.Claims;
using SocialApp.AuthService.Domain.Entities;

namespace SocialApp.AuthService.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken(User user);
    DateTime GetAccessTokenExpiresAt();
    DateTime GetRefreshTokenExpiresAt();
    ClaimsPrincipal? ValidateToken(string token, string expectedTokenType, bool validateLifetime = true);
}