using SocialApp.UserService.Domain.Entities;

namespace SocialApp.UserService.Domain.Repositories;

public interface IUserRepository
{
    Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserProfile?> GetByAuthUserIdAsync(Guid authUserId, CancellationToken cancellationToken = default);
    Task<UserProfile?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task AddAsync(UserProfile userProfile, CancellationToken cancellationToken = default);
    void Update(UserProfile userProfile);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
