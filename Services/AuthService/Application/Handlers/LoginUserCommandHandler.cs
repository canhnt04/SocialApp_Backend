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

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResponse>
{
    private readonly IAuthRepository _repository;
    private readonly IConfiguration _configuration;

    public LoginUserCommandHandler(IAuthRepository repository, IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
    }

    public async Task<AuthResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByUsernameOrEmailAsync(request.UsernameOrEmail, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Tên đăng nhập, email hoặc mật khẩu không hợp lệ");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Tài khoản đã bị khóa");

        // Cập nhật thời gian hoạt động (Chuyển sang UserService quản lý profile)
        user.UpdatedAt = DateTime.UtcNow;

        // Tạo token
        var accessToken = GenerateJwtToken(user, "AccessToken");
        var refreshTokenValue = GenerateJwtToken(user, "RefreshToken");
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var refreshExpiresAt = DateTime.UtcNow.AddDays(int.Parse(jwtSettings["RefreshTokenExpirationDays"] ?? "7"));

        var refreshToken = new RefreshToken
        {
            Token = refreshTokenValue,
            ExpiresAt = refreshExpiresAt,
            UserId = user.Id
        };

        await _repository.AddRefreshTokenAsync(refreshToken, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return new AuthResponse(accessToken, refreshTokenValue, refreshExpiresAt);
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
