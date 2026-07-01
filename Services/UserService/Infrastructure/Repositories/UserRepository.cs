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

    public async Task AddAsync(UserProfile userProfile, CancellationToken cancellationToken = default)
        => await _context.UserProfiles.AddAsync(userProfile, cancellationToken);

    public void Update(UserProfile userProfile)
        => _context.UserProfiles.Update(userProfile);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
