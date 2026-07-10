using MediatR;

namespace SocialApp.UserService.Application.Features.GetFollowing;

public record GetFollowingQuery(Guid UserId) : IRequest<GetFollowingResponse>;
