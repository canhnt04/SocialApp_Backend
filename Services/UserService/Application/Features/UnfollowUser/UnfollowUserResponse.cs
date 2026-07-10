namespace SocialApp.UserService.Application.Features.UnfollowUser;

public record UnfollowUserResponse(
    Guid TargetUserId,
    bool IsFollowing,
    string Message
);
