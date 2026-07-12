using SocialApp.PostService.Domain.Entities;

namespace SocialApp.PostService.Domain.Repositories;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Post>> GetByAuthorIdAsync(Guid authorId, int take = 20, int skip = 0, CancellationToken cancellationToken = default);
    Task<IEnumerable<Post>> GetFeedAsync(int take = 20, int skip = 0, CancellationToken cancellationToken = default);
    Task AddAsync(Post post, CancellationToken cancellationToken = default);
    void Update(Post post);
    void Delete(Post post);
    Task AddMediaAsync(PostMedia media, CancellationToken cancellationToken = default);
    Task<Like?> GetLikeAsync(Guid authorId, Guid postId, CancellationToken cancellationToken = default);
    Task AddLikeAsync(Like like, CancellationToken cancellationToken = default);
    void UpdateLike(Like like);
    Task<Comment?> GetCommentByIdAsync(Guid commentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(Guid postId, CancellationToken cancellationToken = default);
    Task AddCommentAsync(Comment comment, CancellationToken cancellationToken = default);
    void DeleteComment(Comment comment);
    void UpdateComment(Comment comment);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
