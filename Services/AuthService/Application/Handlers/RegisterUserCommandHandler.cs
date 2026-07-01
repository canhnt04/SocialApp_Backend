using MediatR;
using SocialApp.AuthService.Application.Commands;
using SocialApp.AuthService.Application.DTOs;
using SocialApp.AuthService.Domain.Entities;
using SocialApp.AuthService.Domain.Repositories;
using SocialApp.Shared.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace SocialApp.AuthService.Application.Handlers;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponse>
{
    private readonly IAuthRepository _repository;
    private readonly IConfiguration _configuration;
    private readonly MessageBroker? _messageBroker;

    public RegisterUserCommandHandler(IAuthRepository repository, IConfiguration configuration, MessageBroker? messageBroker = null)
    {
        _repository = repository;
        _configuration = configuration;
        _messageBroker = messageBroker;
    }

    public async Task<AuthResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra tính duy nhất
        if (await _repository.ExistsByUsernameAsync(request.Username, cancellationToken))
            throw new InvalidOperationException("Tên đăng nhập đã tồn tại");
        if (await _repository.ExistsByEmailAsync(request.Email, cancellationToken))
            throw new InvalidOperationException("Email đã tồn tại");
        if (await _repository.ExistsByPhoneAsync(request.Phone, cancellationToken))
            throw new InvalidOperationException("Số điện thoại đã tồn tại");

        var user = new User
        {
            Username = request.Username,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Dob = DateOnly.FromDateTime(request.Dob),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        await _repository.AddUserAsync(user, cancellationToken);

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

        // Phát sự kiện qua RabbitMQ
        try
        {
            _messageBroker?.Publish("auth.user.registered", JsonSerializer.Serialize(new
            {
                UserId = user.Id,
                user.Username,
                user.Email,
                RegisteredAt = DateTime.UtcNow
            }));
        }
        catch
        {
            // Ghi log lỗi nhưng không ảnh hưởng luồng chính
        }

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
