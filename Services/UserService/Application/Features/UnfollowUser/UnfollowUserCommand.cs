using MediatR;

namespace SocialApp.UserService.Application.Features.UnfollowUser;

public record UnfollowUserCommand(Guid TargetUserId) : IRequest<UnfollowUserResponse>;
