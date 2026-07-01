using MediatR;
using SocialApp.ChatService.Application.Commands;
using SocialApp.ChatService.Application.DTOs;
using SocialApp.ChatService.Domain.Entities;
using SocialApp.ChatService.Domain.Repositories;
using SocialApp.Shared.Utilities;
using System.Text.Json;

namespace SocialApp.ChatService.Application.Handlers;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, MessageDto>
{
    private readonly IChatRepository _repository;
    private readonly MessageBroker? _messageBroker;

    public SendMessageCommandHandler(IChatRepository repository, MessageBroker? messageBroker = null)
    {
        _repository = repository;
        _messageBroker = messageBroker;
    }

    public async Task<MessageDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var message = new Message
        {
            Content = request.Content,
            SenderId = request.SenderId,
            SenderUsername = request.SenderUsername,
            RecipientId = request.RecipientId,
            ChatGroupId = request.ChatGroupId
        };

        await _repository.AddMessageAsync(message, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        // Phát sự kiện qua RabbitMQ
        try
        {
            _messageBroker?.Publish("chat.message.sent", JsonSerializer.Serialize(new
            {
                MessageId = message.Id,
                message.SenderId,
                message.SenderUsername,
                message.RecipientId,
                message.ChatGroupId,
                SentAt = DateTime.UtcNow
            }));
        }
        catch { /* Ghi log nhưng không ảnh hưởng luồng chính */ }

        return new MessageDto(
            message.Id, message.Content, message.SenderId, message.SenderUsername,
            message.RecipientId, message.ChatGroupId, message.IsRead, message.CreatedAt
        );
    }
}
