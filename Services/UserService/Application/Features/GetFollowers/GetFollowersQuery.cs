using MediatR;

namespace SocialApp.UserService.Application.Features.GetFollowers;

public record GetFollowersQuery(Guid UserId) : IRequest<GetFollowersResponse>;
