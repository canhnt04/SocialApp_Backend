using MediatR;
using SocialApp.UserService.Domain.Repositories;
using SocialApp.UserService.Infrastructure.Authentication;

namespace SocialApp.UserService.Application.Features.UnfollowUser;

public class UnfollowUserHandler : IRequestHandler<UnfollowUserCommand, UnfollowUserResponse>
{
    private readonly IUserRepository _repository;
    private readonly ICurrentUser _currentUser;

    public UnfollowUserHandler(IUserRepository repository, ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<UnfollowUserResponse> Handle(UnfollowUserCommand request, CancellationToken cancellationToken)
    {
        var authUserId = _currentUser.Id
            ?? throw new UnauthorizedAccessException("JWT không hợp lệ hoặc thiếu claim sub");

        var currentProfile = await _repository.GetByAuthUserIdAsync(authUserId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy hồ sơ người dùng hiện tại");

        if (currentProfile.Id == request.TargetUserId)
        {
            throw new ArgumentException("Không thể unfollow chính mình");
        }

        var targetProfile = await _repository.GetByIdAsync(request.TargetUserId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng cần bỏ theo dõi");

        var follow = await _repository.GetFollowAsync(currentProfile.Id, targetProfile.Id, cancellationToken);

        if (follow is not null)
        {
            follow.IsActive = false;
            _repository.UpdateFollow(follow);
            await _repository.SaveChangesAsync(cancellationToken);
        }

        return new UnfollowUserResponse(
            targetProfile.Id,
            false,
            $"Đã bỏ theo dõi {targetProfile.Username}"
        );
    }
}
