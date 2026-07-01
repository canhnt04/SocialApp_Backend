namespace SocialApp.UserService.Application.DTOs;

public record UpdateUserDto(
    string? FirstName,
    string? LastName,
    string? Phone,
    string? Avatar,
    DateOnly? Dob,
    string? Bio,
    string? Location,
    string? Website
);

public record UserProfileDto(
    Guid Id,
    Guid AuthUserId,
    string Username,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Avatar,
    DateOnly? Dob,
    string? Bio,
    string? Location,
    string? Website,
    bool IsActive,
    DateTime? LastActiveAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
