using MediatR;
using SocialApp.UserService.Domain.Repositories;

namespace SocialApp.UserService.Application.Features.GetFollowers;

public class GetFollowersHandler : IRequestHandler<GetFollowersQuery, GetFollowersResponse>
{
    private readonly IUserRepository _repository;

    public GetFollowersHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetFollowersResponse> Handle(GetFollowersQuery request, CancellationToken cancellationToken)
    {
        var followers = await _repository.GetFollowersAsync(request.UserId, cancellationToken);

        var items = followers.Select(user => new GetFollowersUserItem(
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

        return new GetFollowersResponse(items);
    }
}
