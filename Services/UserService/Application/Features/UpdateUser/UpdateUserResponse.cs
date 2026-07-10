namespace SocialApp.UserService.Application.Features.UpdateUser;


public record UpdateUserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string? Avatar,
    DateTime UpdatedAt
);

