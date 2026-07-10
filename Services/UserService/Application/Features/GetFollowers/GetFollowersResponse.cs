namespace SocialApp.UserService.Application.Features.GetFollowers;

public record GetFollowersResponse(List<GetFollowersUserItem> Followers);

public record GetFollowersUserItem(
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
