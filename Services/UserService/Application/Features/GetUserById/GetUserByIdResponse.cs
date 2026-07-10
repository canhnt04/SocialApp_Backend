namespace SocialApp.UserService.Application.Features.GetUserById;

public record GetUserByIdResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Avatar,
    DateOnly? Dob,
    string? Bio,
    string? Location,
    string? Website
);
