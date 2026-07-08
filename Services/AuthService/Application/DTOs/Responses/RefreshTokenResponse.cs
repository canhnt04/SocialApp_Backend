namespace SocialApp.AuthService.Application.DTOs.Responses;

public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);