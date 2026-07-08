using MediatR;
using SocialApp.ChatService.Application.DTOs;

namespace SocialApp.ChatService.Application.Commands;

public record CreateOrGetPrivateChatCommand(
    Guid UserId1,
    Guid UserId2,
    Guid CurrentUserId
) : IRequest<PrivateChatDto>;
