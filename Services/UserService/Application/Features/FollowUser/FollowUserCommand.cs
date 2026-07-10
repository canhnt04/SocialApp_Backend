using MediatR;

namespace SocialApp.UserService.Application.Features.FollowUser;

public record FollowUserCommand(Guid TargetUserId) : IRequest<FollowUserResponse>;
