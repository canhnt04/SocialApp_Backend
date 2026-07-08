using MediatR;
using SocialApp.UserService.Application.Commands;
using SocialApp.UserService.Application.DTOs;
using SocialApp.UserService.Domain.Repositories;
using SocialApp.UserService.Infrastructure.Authentication;

namespace SocialApp.UserService.Application.Handlers;

public class UnfollowUserCommandHandler : IRequestHandler<UnfollowUserCommand, FollowActionResponseDto>
{
    private readonly IUserRepository _repository;
    private readonly ICurrentUser _currentUser;

    public UnfollowUserCommandHandler(IUserRepository repository, ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<FollowActionResponseDto> Handle(UnfollowUserCommand request, CancellationToken cancellationToken)
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

        return new FollowActionResponseDto(
            targetProfile.Id,
            false,
            $"Đã bỏ theo dõi {targetProfile.Username}"
        );
    }
}