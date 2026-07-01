namespace SocialApp.AuthService.Application.DTOs;

public record RegisterRequest(
    string Username,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateTime Dob,
    string Password
);

public record LoginRequest(
    string UsernameOrEmail,
    string Password
);

public record RefreshTokenRequest(
    string RefreshToken
);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
