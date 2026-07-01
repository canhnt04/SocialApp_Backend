using SocialApp.Shared.Base;

namespace SocialApp.ChatService.Domain.Entities;

public class Message : BaseEntity
{
    public string Content { get; set; } = default!;
    public Guid SenderId { get; set; }
    public string SenderUsername { get; set; } = default!;

    /// <summary>
    /// Null nếu là tin nhắn nhóm
    /// </summary>
    public Guid? RecipientId { get; set; }

    /// <summary>
    /// Null nếu là tin nhắn cá nhân
    /// </summary>
    public Guid? ChatGroupId { get; set; }
    public ChatGroup? ChatGroup { get; set; }

    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
}
