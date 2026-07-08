using MediatR;
using SocialApp.UserService.Application.DTOs;

namespace SocialApp.UserService.Application.Commands;

public record FollowUserCommand(Guid TargetUserId) : IRequest<FollowActionResponseDto>;