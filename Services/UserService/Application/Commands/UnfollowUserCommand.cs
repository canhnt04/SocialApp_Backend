using MediatR;
using SocialApp.UserService.Application.DTOs;

namespace SocialApp.UserService.Application.Commands;

public record UnfollowUserCommand(Guid TargetUserId) : IRequest<FollowActionResponseDto>;