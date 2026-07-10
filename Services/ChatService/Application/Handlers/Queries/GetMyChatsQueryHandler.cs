using MediatR;
using SocialApp.ChatService.Application.DTOs;
using SocialApp.ChatService.Application.Queries;
using SocialApp.ChatService.Domain.Repositories;

namespace SocialApp.ChatService.Application.Handlers.Queries;

public class GetMyChatsQueryHandler : IRequestHandler<GetMyChatsQuery, IEnumerable<ChatListDto>>
{
    private readonly IChatRepository _repository;

    public GetMyChatsQueryHandler(IChatRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ChatListDto>> Handle(GetMyChatsQuery request, CancellationToken cancellationToken)
    {
        var chats = await _repository.GetUserChatsAsync(request.CurrentUserId, request.Take, request.Skip, cancellationToken);

        return chats.Select(c => new ChatListDto(
            c.Id,
            c.Type.ToString(),
            c.Name ?? string.Empty,
            c.Members.Select(m => m.UserId).ToList(),
            c.CreatedAt,
            c.IsActive
        ));
    }
}
