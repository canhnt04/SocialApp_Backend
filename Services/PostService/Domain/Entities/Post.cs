using SocialApp.PostService.Domain.Base;

namespace SocialApp.PostService.Domain.Entities;

public class Post : BaseEntity
{
    public Guid AuthorId { get; set; }
    public string AuthorUsername { get; set; } = default!;
    public Guid? OriginalPostId { get; set; }
    public string Content { get; set; } = default!;
    public int LikeCount { get; set; } = 0;
    public int CommentCount { get; set; } = 0;
    public int ShareCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public PostVisibility Visibility { get; set; } = PostVisibility.Public;

    // Navigation
    public ICollection<PostMedia> Media { get; set; } = new List<PostMedia>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Like> Likes { get; set; } = new List<Like>();
}

public enum PostVisibility
{
    Public = 0,
    FriendsOnly = 1,
    Private = 2
}
