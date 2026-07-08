using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using MediatR;
using SocialApp.ChatService.Application.DTOs;
using SocialApp.ChatService.Application.Commands;
using SocialApp.ChatService.Domain.Entities;
using SocialApp.ChatService.Domain.Repositories;

namespace SocialApp.ChatService.Hubs;

/// <summary>
/// SignalR Hub cho chat thời gian thực
/// Hỗ trợ chat 1-1 và chat nhóm
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly IChatRepository _repository;
    private readonly IMediator _mediator;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(IChatRepository repository, IMediator mediator, ILogger<ChatHub> logger)
    {
        _repository = repository;
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Tham gia phòng chat (Private hoặc Group)
    /// </summary>
    public async Task JoinChat(string chatId)
    {
        if (!TryGetCurrentUser(out var userId, out _))
        {
            throw new HubException("Unauthorized user identity.");
        }

        if (!Guid.TryParse(chatId, out var parsedChatId))
        {
            throw new HubException("Invalid chatId.");
        }

        var chat = await _repository.GetChatByIdAsync(parsedChatId);
        if (chat == null)
        {
            throw new HubException("Chat room not found.");
        }

        if (!chat.IsActive)
        {
            throw new HubException("Chat room is deactivated.");
        }

        var isMember = chat.Members.Any(m => m.UserId == userId);
        if (!isMember)
        {
            throw new HubException("Forbidden. You are not a member of this chat.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
        await Clients.Group(chatId).SendAsync("UserJoined", userId.ToString(), chatId);
    }

    /// <summary>
    /// Rời khỏi phòng chat
    /// </summary>
    public async Task LeaveChat(string chatId)
    {
        if (!TryGetCurrentUser(out var userId, out _))
        {
            throw new HubException("Unauthorized user identity.");
        }

        if (!Guid.TryParse(chatId, out var parsedChatId))
        {
            throw new HubException("Invalid chatId.");
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
        await Clients.Group(chatId).SendAsync("UserLeft", userId.ToString(), chatId);
    }

    /// <summary>
    /// Gửi tin nhắn dùng chung cho cả Private và Group chat
    /// </summary>
    public async Task SendMessage(string chatId, string content)
    {
        if (!TryGetCurrentUser(out var senderId, out var resolvedUsername))
        {
            throw new HubException("Unauthorized user identity.");
        }

        if (!Guid.TryParse(chatId, out var parsedChatId))
        {
            throw new HubException("Invalid chatId.");
        }

        try
        {
            var command = new SendMessageCommand(content, senderId, resolvedUsername, parsedChatId);
            var messageDto = await _mediator.Send(command);

            // Broadcast tới tất cả những người khác trong phòng chat
            await Clients.OthersInGroup(chatId).SendAsync("ReceiveMessage", messageDto);
            // Gửi báo nhận tin nhắn về cho caller
            await Clients.Caller.SendAsync("MessageSent", messageDto);
        }
        catch (ArgumentException ex)
        {
            throw new HubException(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            throw new HubException(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            throw new HubException(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new HubException(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error handling SendMessage for chatId {chatId}");
            throw new HubException("An error occurred while processing your request.");
        }
    }

    /// <summary>
    /// Thông báo đang gõ
    /// </summary>
    public async Task Typing(string chatId)
    {
        if (!TryGetCurrentUser(out var senderId, out var resolvedUsername))
        {
            return;
        }

        if (!Guid.TryParse(chatId, out var parsedChatId))
        {
            return;
        }

        var chat = await _repository.GetChatByIdAsync(parsedChatId);
        if (chat == null || !chat.IsActive) return;

        var isMember = chat.Members.Any(m => m.UserId == senderId);
        if (!isMember) return;

        await Clients.OthersInGroup(chatId).SendAsync("UserTyping", resolvedUsername, chatId);
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    private bool TryGetCurrentUser(out Guid userId, out string username)
    {
        userId = Guid.Empty;
        username = Context.User?.FindFirst(ClaimTypes.Name)?.Value
                   ?? Context.User?.Identity?.Name
                   ?? "unknown";

        var rawUserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? Context.User?.FindFirst("sub")?.Value
                        ?? Context.UserIdentifier;

        return Guid.TryParse(rawUserId, out userId) && userId != Guid.Empty;
    }
}
