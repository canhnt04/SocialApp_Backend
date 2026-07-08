using Microsoft.EntityFrameworkCore;
using SocialApp.UserService.Domain.Entities;
using SocialApp.UserService.Domain.Repositories;
using SocialApp.UserService.Infrastructure.Data;

namespace SocialApp.UserService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _context;

    public UserRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.UserProfiles.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<UserProfile?> GetByAuthUserIdAsync(Guid authUserId, CancellationToken cancellationToken = default)
        => await _context.UserProfiles.FirstOrDefaultAsync(u => u.AuthUserId == authUserId, cancellationToken);

    public async Task<UserProfile?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        => await _context.UserProfiles.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

    public async Task<Follow?> GetFollowAsync(Guid followerId, Guid followingId, CancellationToken cancellationToken = default)
        => await _context.Follows.FirstOrDefaultAsync(
            f => f.FollowerId == followerId && f.FollowingId == followingId,
            cancellationToken);

    public async Task<List<UserProfile>> GetFollowersAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.Follows
            .AsNoTracking()
            .Where(f => f.FollowingId == userId && f.IsActive && f.Status == FollowStatus.Accepted)
            .Include(f => f.Follower)
            .Select(f => f.Follower!)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<List<UserProfile>> GetFollowingAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.Follows
            .AsNoTracking()
            .Where(f => f.FollowerId == userId && f.IsActive && f.Status == FollowStatus.Accepted)
            .Include(f => f.Following)
            .Select(f => f.Following!)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(UserProfile userProfile, CancellationToken cancellationToken = default)
        => await _context.UserProfiles.AddAsync(userProfile, cancellationToken);

    public async Task AddFollowAsync(Follow follow, CancellationToken cancellationToken = default)
        => await _context.Follows.AddAsync(follow, cancellationToken);

    public void Update(UserProfile userProfile)
        => _context.UserProfiles.Update(userProfile);

    public void UpdateFollow(Follow follow)
        => _context.Follows.Update(follow);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
