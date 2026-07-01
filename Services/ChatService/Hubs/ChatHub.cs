using Microsoft.AspNetCore.SignalR;
using SocialApp.ChatService.Application.DTOs;
using SocialApp.ChatService.Domain.Entities;
using SocialApp.ChatService.Domain.Repositories;

namespace SocialApp.ChatService.Hubs;

/// <summary>
/// SignalR Hub cho chat thời gian thực
/// Hỗ trợ chat 1-1 và chat nhóm
/// </summary>
public class ChatHub : Hub
{
    private readonly IChatRepository _repository;

    public ChatHub(IChatRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Gửi tin nhắn cá nhân (1-1)
    /// </summary>
    public async Task SendPrivateMessage(string recipientId, string senderUsername, string content)
    {
        var senderId = Context.UserIdentifier ?? Context.ConnectionId;

        var message = new Message
        {
            Content = content,
            SenderId = Guid.Parse(senderId),
            SenderUsername = senderUsername,
            RecipientId = Guid.Parse(recipientId)
        };

        await _repository.AddMessageAsync(message);
        await _repository.SaveChangesAsync();

        var messageDto = new MessageDto(
            message.Id, message.Content, message.SenderId, message.SenderUsername,
            message.RecipientId, null, false, message.CreatedAt
        );

        // Gửi tới người nhận
        await Clients.User(recipientId).SendAsync("ReceiveMessage", messageDto);
        // Gửi xác nhận cho người gửi
        await Clients.Caller.SendAsync("MessageSent", messageDto);
    }

    /// <summary>
    /// Gửi tin nhắn nhóm
    /// </summary>
    public async Task SendGroupMessage(string groupId, string senderUsername, string content)
    {
        var senderId = Context.UserIdentifier ?? Context.ConnectionId;

        var message = new Message
        {
            Content = content,
            SenderId = Guid.Parse(senderId),
            SenderUsername = senderUsername,
            ChatGroupId = Guid.Parse(groupId)
        };

        await _repository.AddMessageAsync(message);
        await _repository.SaveChangesAsync();

        var messageDto = new MessageDto(
            message.Id, message.Content, message.SenderId, message.SenderUsername,
            null, message.ChatGroupId, false, message.CreatedAt
        );

        // Gửi tới tất cả thành viên trong nhóm
        await Clients.Group(groupId).SendAsync("ReceiveGroupMessage", messageDto);
    }

    /// <summary>
    /// Tham gia nhóm chat
    /// </summary>
    public async Task JoinGroup(string groupId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
        await Clients.Group(groupId).SendAsync("UserJoined", Context.UserIdentifier, groupId);
    }

    /// <summary>
    /// Rời nhóm chat
    /// </summary>
    public async Task LeaveGroup(string groupId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
        await Clients.Group(groupId).SendAsync("UserLeft", Context.UserIdentifier, groupId);
    }

    /// <summary>
    /// Thông báo đang gõ
    /// </summary>
    public async Task Typing(string recipientId, string senderUsername)
    {
        await Clients.User(recipientId).SendAsync("UserTyping", senderUsername);
    }

    /// <summary>
    /// Thông báo đang gõ trong nhóm
    /// </summary>
    public async Task TypingInGroup(string groupId, string senderUsername)
    {
        await Clients.OthersInGroup(groupId).SendAsync("UserTypingInGroup", senderUsername, groupId);
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
