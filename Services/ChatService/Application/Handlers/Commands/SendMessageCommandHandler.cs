using MediatR;
using Microsoft.Extensions.Logging;
using SocialApp.ChatService.Application.Commands;
using SocialApp.ChatService.Application.DTOs;
using SocialApp.ChatService.Application.Interfaces;
using SocialApp.ChatService.Domain.Entities;
using SocialApp.ChatService.Domain.Repositories;
using System.Text.Json;

namespace SocialApp.ChatService.Application.Handlers.Commands;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, MessageDto>
{
    private readonly IChatRepository _repository;
    private readonly IMessagePublisher? _messagePublisher;
    private readonly ILogger<SendMessageCommandHandler>? _logger;

    public SendMessageCommandHandler(
        IChatRepository repository,
        IMessagePublisher? messagePublisher = null,
        ILogger<SendMessageCommandHandler>? logger = null)
    {
        _repository = repository;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task<MessageDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        // 1. Validation
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException("Message content cannot be empty.");
        }

        var chat = await _repository.GetChatByIdAsync(request.ChatId, cancellationToken);
        if (chat == null)
        {
            throw new KeyNotFoundException("Chat room not found.");
        }

        if (!chat.IsActive)
        {
            throw new InvalidOperationException("Chat room is deactivated.");
        }

        var isMember = chat.Members.Any(m => m.UserId == request.SenderId);
        if (!isMember)
        {
            throw new UnauthorizedAccessException("Forbidden. You are not a member of this chat.");
        }

        // 2. Business logic
        var message = new Message
        {
            Content = request.Content,
            SenderId = request.SenderId,
            SenderUsername = request.SenderUsername,
            ChatId = request.ChatId
        };

        await _repository.AddMessageAsync(message, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger?.LogInformation($"Message saved: Id={message.Id}, ChatId={message.ChatId}");

        // Phát sự kiện qua RabbitMQ
        if (_messagePublisher != null)
        {
            try
            {
                var eventData = new
                {
                    MessageId = message.Id,
                    message.SenderId,
                    message.SenderUsername,
                    message.ChatId,
                    SentAt = DateTime.UtcNow
                };

                _messagePublisher.Publish("chat.message.sent", JsonSerializer.Serialize(eventData));

                _logger?.LogInformation($"Message published to RabbitMQ: MessageId={message.Id}, RoutingKey=chat.message.sent");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Failed to publish message to RabbitMQ: MessageId={message.Id}, Error={ex.Message}");
            }
        }
        else
        {
            _logger?.LogWarning("MessagePublisher is not configured");
        }

        return new MessageDto(
            message.Id, message.Content, message.SenderId, message.SenderUsername,
            message.ChatId, message.IsRead, message.CreatedAt
        );
    }
}
