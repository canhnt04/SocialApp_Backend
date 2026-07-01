using SocialApp.Shared.Base;

namespace SocialApp.ChatService.Domain.Entities;

public class ChatGroup : BaseEntity
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public Guid CreatedByUserId { get; set; }

    // Danh sách thành viên (lưu dưới dạng JSON hoặc bảng riêng)
    public ICollection<ChatGroupMember> Members { get; set; } = new List<ChatGroupMember>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

public class ChatGroupMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChatGroupId { get; set; }
    public ChatGroup ChatGroup { get; set; } = default!;
    public Guid UserId { get; set; }
    public string Username { get; set; } = default!;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public bool IsAdmin { get; set; } = false;
}
