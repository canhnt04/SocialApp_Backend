using SocialApp.Shared.Base;

namespace SocialApp.PostService.Domain.Entities;

public class Comment : BaseEntity
{
    public Guid AuthorId { get; set; }
    public Guid PostId { get; set; }
    public Guid? ParentId { get; set; }
    public string Content { get; set; } = default!;
    public bool IsActive { get; set; } = true;

    // Navigation
    public Post Post { get; set; } = default!;
    public Comment? Parent { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    public ICollection<Like> Likes { get; set; } = new List<Like>();
}
