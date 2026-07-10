using MediatR;
using SocialApp.ChatService.Application.DTOs;
using SocialApp.ChatService.Application.Queries;
using SocialApp.ChatService.Domain.Repositories;

namespace SocialApp.ChatService.Application.Handlers.Queries;

public class GetChatMessagesQueryHandler : IRequestHandler<GetChatMessagesQuery, IEnumerable<MessageDto>>
{
    private readonly IChatRepository _repository;

    public GetChatMessagesQueryHandler(IChatRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<MessageDto>> Handle(GetChatMessagesQuery request, CancellationToken cancellationToken)
    {
        var chat = await _repository.GetChatByIdAsync(request.ChatId, cancellationToken);
        if (chat == null)
        {
            throw new KeyNotFoundException("Chat room not found.");
        }

        var isMember = await _repository.IsUserInChatAsync(request.ChatId, request.CurrentUserId, cancellationToken);
        if (!isMember)
        {
            throw new UnauthorizedAccessException("You do not have permission to access messages in this chat.");
        }

        var messages = await _repository.GetChatMessagesAsync(request.ChatId, request.Take, request.Skip, cancellationToken);

        return messages.Select(m => new MessageDto(
            m.Id, m.Content, m.SenderId, m.SenderUsername,
            m.ChatId, m.IsRead, m.CreatedAt
        ));
    }
}
