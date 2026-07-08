using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SocialApp.AuthService.Application.Interfaces;
using SocialApp.AuthService.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SocialApp.AuthService.Infrastructure.Security;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(User user)
        => GenerateToken(user, "AccessToken", GetAccessTokenExpiresAt());

    public string GenerateRefreshToken(User user)
        => GenerateToken(user, "RefreshToken", GetRefreshTokenExpiresAt());

    public DateTime GetAccessTokenExpiresAt()
        => DateTime.UtcNow.AddMinutes(GetIntSetting("AccessTokenExpirationMinutes", 15));

    public DateTime GetRefreshTokenExpiresAt()
        => DateTime.UtcNow.AddDays(GetIntSetting("RefreshTokenExpirationDays", 7));

    public ClaimsPrincipal? ValidateToken(string token, string expectedTokenType, bool validateLifetime = true)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secret = jwtSettings["SecretKey"] ?? "SuperSecretKey!";
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = validateLifetime,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken)
                return null;

            var tokenType = jwtToken.Claims.FirstOrDefault(c => c.Type == "typ")?.Value;
            return string.Equals(tokenType, expectedTokenType, StringComparison.Ordinal)
                ? principal
                : null;
        }
        catch
        {
            return null;
        }
    }

    private string GenerateToken(User user, string tokenType, DateTime expiresAt)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secret = jwtSettings["SecretKey"] ?? "SuperSecretKey!";
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim("typ", tokenType)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private int GetIntSetting(string key, int defaultValue)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        return int.TryParse(jwtSettings[key], out var value) ? value : defaultValue;
    }
}