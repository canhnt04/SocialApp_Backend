using MediatR;
using SocialApp.UserService.Application.DTOs;

namespace SocialApp.UserService.Application.Commands;

public record UpdateUserCommand(
    Guid UserId,
    UpdateUserDto Data
) : IRequest<UserProfileDto>;
