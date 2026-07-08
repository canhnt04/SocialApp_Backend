using SocialApp.UserService.Domain.Entities;

namespace SocialApp.UserService.Domain.Repositories;

public interface IUserRepository
{
    Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserProfile?> GetByAuthUserIdAsync(Guid authUserId, CancellationToken cancellationToken = default);
    Task<UserProfile?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<Follow?> GetFollowAsync(Guid followerId, Guid followingId, CancellationToken cancellationToken = default);
    Task<List<UserProfile>> GetFollowersAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<UserProfile>> GetFollowingAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(UserProfile userProfile, CancellationToken cancellationToken = default);
    Task AddFollowAsync(Follow follow, CancellationToken cancellationToken = default);
    void Update(UserProfile userProfile);
    void UpdateFollow(Follow follow);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
