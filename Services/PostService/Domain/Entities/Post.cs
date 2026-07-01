using SocialApp.Shared.Base;

namespace SocialApp.PostService.Domain.Entities;

public class Post : BaseEntity
{
    public Guid AuthorId { get; set; }
    public string AuthorUsername { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public int LikeCount { get; set; } = 0;
    public int CommentCount { get; set; } = 0;
    public int ShareCount { get; set; } = 0;
    public bool IsPublished { get; set; } = true;
    public PostVisibility Visibility { get; set; } = PostVisibility.Public;
}

public enum PostVisibility
{
    Public = 0,
    FriendsOnly = 1,
    Private = 2
}
