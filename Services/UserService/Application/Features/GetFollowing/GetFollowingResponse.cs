namespace SocialApp.UserService.Application.Features.GetFollowing;

public record GetFollowingResponse(List<GetFollowingUserItem> Following);

public record GetFollowingUserItem(
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
