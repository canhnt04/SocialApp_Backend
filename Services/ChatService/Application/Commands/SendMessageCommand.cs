using MediatR;
using SocialApp.ChatService.Application.DTOs;

namespace SocialApp.ChatService.Application.Commands;

public record SendMessageCommand(
    string Content,
    Guid SenderId,
    string SenderUsername,
    Guid ChatId
) : IRequest<MessageDto>;
