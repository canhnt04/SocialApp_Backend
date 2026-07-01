using SocialApp.ChatService.Domain.Base;

namespace SocialApp.ChatService.Domain.Entities;

public class Message : BaseEntity
{
    public string Content { get; set; } = default!;
    public Guid SenderId { get; set; }
    public string SenderUsername { get; set; } = default!;

    public Guid ChatId { get; set; }
    public Chat Chat { get; set; } = default!;

    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
}
