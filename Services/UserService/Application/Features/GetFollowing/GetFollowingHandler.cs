using MediatR;
using SocialApp.UserService.Domain.Repositories;

namespace SocialApp.UserService.Application.Features.GetFollowing;

public class GetFollowingHandler : IRequestHandler<GetFollowingQuery, GetFollowingResponse>
{
    private readonly IUserRepository _repository;

    public GetFollowingHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetFollowingResponse> Handle(GetFollowingQuery request, CancellationToken cancellationToken)
    {
        var following = await _repository.GetFollowingAsync(request.UserId, cancellationToken);

        var items = following.Select(user => new GetFollowingUserItem(
            user.Id,
            user.AuthUserId,
            user.Username,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Phone,
            user.Avatar,
            user.Dob,
            user.Bio,
            user.Location,
            user.Website,
            user.IsActive,
            user.LastActiveAt,
            user.CreatedAt,
            user.UpdatedAt
        )).ToList();

        return new GetFollowingResponse(items);
    }
}
