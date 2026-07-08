using MediatR;
using SocialApp.ChatService.Application.DTOs;

namespace SocialApp.ChatService.Application.Queries;

public record GetMyChatsQuery(
    Guid CurrentUserId,
    int Take = 50,
    int Skip = 0
) : IRequest<IEnumerable<ChatListDto>>;
