namespace SocialApp.AuthService.Application.DTOs.Requests;

public record LoginRequest(
    string UsernameOrEmail,
    string Password
);