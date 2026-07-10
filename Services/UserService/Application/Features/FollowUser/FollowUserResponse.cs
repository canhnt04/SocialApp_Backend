namespace SocialApp.UserService.Application.Features.FollowUser;

public record FollowUserResponse(
    Guid TargetUserId,
    bool IsFollowing,
    string Message
);
