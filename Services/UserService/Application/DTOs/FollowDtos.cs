namespace SocialApp.UserService.Application.DTOs;

public record FollowActionResponseDto(
    Guid TargetUserId,
    bool IsFollowing,
    string Message
);