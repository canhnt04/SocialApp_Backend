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
            .Where(p => p.AuthorId == authorId && p.IsPublished)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Post>> GetFeedAsync(
        int take = 20, int skip = 0, CancellationToken cancellationToken = default)
        => await _context.Posts
            .Where(p => p.IsPublished && p.Visibility == PostVisibility.Public)
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

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
