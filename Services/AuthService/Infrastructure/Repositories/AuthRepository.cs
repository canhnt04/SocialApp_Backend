using Microsoft.EntityFrameworkCore;
using SocialApp.AuthService.Domain.Entities;
using SocialApp.AuthService.Domain.Repositories;
using SocialApp.AuthService.Infrastructure.Data;

namespace SocialApp.AuthService.Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly AuthDbContext _context;

    public AuthRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        => await _context.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken = default)
        => await _context.Users.FirstOrDefaultAsync(
            u => u.Username == usernameOrEmail || u.Email == usernameOrEmail, cancellationToken);

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
        => await _context.Users.AnyAsync(u => u.Username == username, cancellationToken);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);

    public async Task<bool> ExistsByPhoneAsync(string phone, CancellationToken cancellationToken = default)
        => await _context.Users.AnyAsync(u => u.Phone == phone, cancellationToken);

    public async Task AddUserAsync(User user, CancellationToken cancellationToken = default)
        => await _context.Users.AddAsync(user, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
