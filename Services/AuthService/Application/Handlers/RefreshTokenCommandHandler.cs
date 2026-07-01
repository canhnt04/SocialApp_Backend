using MediatR;
using SocialApp.AuthService.Application.Commands;
using SocialApp.AuthService.Application.DTOs;
using SocialApp.AuthService.Domain.Entities;
using SocialApp.AuthService.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SocialApp.AuthService.Application.Handlers;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IAuthRepository _repository;
    private readonly IConfiguration _configuration;

    public RefreshTokenCommandHandler(IAuthRepository repository, IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existingToken = await _repository.GetRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (existingToken == null)
            throw new UnauthorizedAccessException("Refresh token không hợp lệ");

        if (existingToken.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token đã hết hạn");

        // Thu hồi token cũ
        existingToken.IsRevoked = true;
        existingToken.RevokedAt = DateTime.UtcNow;

        var user = existingToken.User;

        // Tạo token mới
        var newAccessToken = GenerateJwtToken(user, "AccessToken");
        var newRefreshTokenValue = GenerateJwtToken(user, "RefreshToken");
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var refreshExpiresAt = DateTime.UtcNow.AddDays(int.Parse(jwtSettings["RefreshTokenExpirationDays"] ?? "7"));

        var newRefreshToken = new RefreshToken
        {
            Token = newRefreshTokenValue,
            ExpiresAt = refreshExpiresAt,
            UserId = user.Id
        };

        await _repository.AddRefreshTokenAsync(newRefreshToken, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return new AuthResponse(newAccessToken, newRefreshTokenValue, refreshExpiresAt);
    }

    private string GenerateJwtToken(User user, string tokenType)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secret = jwtSettings["SecretKey"] ?? "SuperSecretKeyChangeMe123!";
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = tokenType == "AccessToken"
            ? DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "15"))
            : DateTime.UtcNow.AddDays(int.Parse(jwtSettings["RefreshTokenExpirationDays"] ?? "7"));

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
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
