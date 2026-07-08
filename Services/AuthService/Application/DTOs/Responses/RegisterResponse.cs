namespace SocialApp.AuthService.Application.DTOs.Responses;

public record RegisterResponse(
    Guid UserId,
    string Username
);