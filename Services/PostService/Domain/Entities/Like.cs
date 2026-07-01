using SocialApp.PostService.Domain.Base;

namespace SocialApp.PostService.Domain.Entities;

public class Like : BaseEntity
{
    public Guid AuthorId { get; set; }
    public Guid? PostId { get; set; }
    public Guid? CommentId { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Post? Post { get; set; }
    public Comment? Comment { get; set; }
}
