namespace SocialApp.AuthService.Application.DTOs.Requests;

public record RegisterRequest(
    string Username,
    string Email,
    string Phone,
    string Password
);