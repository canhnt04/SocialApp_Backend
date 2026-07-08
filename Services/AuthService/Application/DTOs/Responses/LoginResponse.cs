namespace SocialApp.AuthService.Application.DTOs.Responses;

public record LoginResponse(
    Guid UserId,
    string Username,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt
);
