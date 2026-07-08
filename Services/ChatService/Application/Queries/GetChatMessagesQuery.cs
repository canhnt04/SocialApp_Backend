using MediatR;
using SocialApp.ChatService.Application.DTOs;

namespace SocialApp.ChatService.Application.Queries;

public record GetChatMessagesQuery(
    Guid ChatId,
    Guid CurrentUserId,
    int Take = 50,
    int Skip = 0
) : IRequest<IEnumerable<MessageDto>>;
