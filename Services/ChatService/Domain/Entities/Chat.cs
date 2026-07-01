using SocialApp.ChatService.Domain.Base;

namespace SocialApp.ChatService.Domain.Entities;

public class Chat : BaseEntity
{
    public string? Name { get; set; }
    public ChatType Type { get; set; } = ChatType.Private;
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<ChatUser> Members { get; set; } = new List<ChatUser>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

public class ChatUser
{
    public Guid ChatId { get; set; }
    public Guid UserId { get; set; }

    // Navigation
    public Chat Chat { get; set; } = default!;
}

public enum ChatType
{
    Private = 0,
    Group = 1
}
