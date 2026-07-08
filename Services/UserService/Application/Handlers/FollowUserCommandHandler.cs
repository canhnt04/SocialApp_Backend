using MediatR;
using SocialApp.UserService.Application.Commands;
using SocialApp.UserService.Application.DTOs;
using SocialApp.UserService.Domain.Entities;
using SocialApp.UserService.Domain.Repositories;
using SocialApp.UserService.Infrastructure.Authentication;

namespace SocialApp.UserService.Application.Handlers;

public class FollowUserCommandHandler : IRequestHandler<FollowUserCommand, FollowActionResponseDto>
{
    private readonly IUserRepository _repository;
    private readonly ICurrentUser _currentUser;

    public FollowUserCommandHandler(IUserRepository repository, ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<FollowActionResponseDto> Handle(FollowUserCommand request, CancellationToken cancellationToken)
    {
        var authUserId = _currentUser.Id
            ?? throw new UnauthorizedAccessException("JWT không hợp lệ hoặc thiếu claim sub");

        var currentProfile = await _repository.GetByAuthUserIdAsync(authUserId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy hồ sơ người dùng hiện tại");

        if (currentProfile.Id == request.TargetUserId)
        {
            throw new ArgumentException("Không thể follow chính mình");
        }

        var targetProfile = await _repository.GetByIdAsync(request.TargetUserId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng cần theo dõi");

        var follow = await _repository.GetFollowAsync(currentProfile.Id, targetProfile.Id, cancellationToken);

        if (follow is null)
        {
            follow = new Follow
            {
                FollowerId = currentProfile.Id,
                FollowingId = targetProfile.Id,
                Status = FollowStatus.Accepted,
                IsActive = true
            };

            await _repository.AddFollowAsync(follow, cancellationToken);
        }
        else
        {
            follow.IsActive = true;
            follow.Status = FollowStatus.Accepted;
            _repository.UpdateFollow(follow);
        }

        await _repository.SaveChangesAsync(cancellationToken);

        return new FollowActionResponseDto(
            targetProfile.Id,
            true,
            $"Đã theo dõi {targetProfile.Username}"
        );
    }
}