using Microsoft.EntityFrameworkCore;
using SocialApp.PostService.Domain.Entities;
using SocialApp.PostService.Domain.Repositories;
using SocialApp.PostService.Infrastructure.Data;

namespace SocialApp.PostService.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly PostDbContext _context;

    public PostRepository(PostDbContext context)
    {
        _context = context;
    }

    public async Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Posts.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IEnumerable<Post>> GetByAuthorIdAsync(
        Guid authorId, int take = 20, int skip = 0, CancellationToken cancellationToken = default)
        => await _context.Posts
            .Where(p => p.AuthorId == authorId && p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Post>> GetFeedAsync(
        int take = 20, int skip = 0, CancellationToken cancellationToken = default)
        => await _context.Posts
            .Where(p => p.IsActive && p.Visibility == PostVisibility.Public)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Post post, CancellationToken cancellationToken = default)
        => await _context.Posts.AddAsync(post, cancellationToken);

    public void Update(Post post)
        => _context.Posts.Update(post);

    public void Delete(Post post)
        => _context.Posts.Remove(post);

    public async Task AddMediaAsync(PostMedia media, CancellationToken cancellationToken = default)
        => await _context.PostMedia.AddAsync(media, cancellationToken);

    public async Task<Like?> GetLikeAsync(Guid authorId, Guid postId, CancellationToken cancellationToken = default)
        => await _context.Likes.FirstOrDefaultAsync(l => l.AuthorId == authorId && l.PostId == postId, cancellationToken);

    public async Task AddLikeAsync(Like like, CancellationToken cancellationToken = default)
        => await _context.Likes.AddAsync(like, cancellationToken);

    public void UpdateLike(Like like)
        => _context.Likes.Update(like);

    public async Task<Comment?> GetCommentByIdAsync(Guid commentId, CancellationToken cancellationToken = default)
    => await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);

    public async Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(Guid postId, CancellationToken cancellationToken = default)
        => await _context.Comments
            .Where(c => c.PostId == postId && c.IsActive)
            .OrderByDescending(c => c.CreatedAt) // Mới nhất xếp lên đầu
            .ToListAsync(cancellationToken);

    public async Task AddCommentAsync(Comment comment, CancellationToken cancellationToken = default)
        => await _context.Comments.AddAsync(comment, cancellationToken);

    public void UpdateComment(Comment comment)
        => _context.Comments.Update(comment);

    public void DeleteComment(Comment comment)
        => _context.Comments.Remove(comment);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
