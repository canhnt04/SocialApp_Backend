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
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
