using SocialApp.Shared.Base;

namespace SocialApp.AuthService.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string? Avatar { get; set; }
    public DateOnly Dob { get; set; }
    public string PasswordHash { get; set; } = default!;
    public string? Bio { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastActiveAt { get; set; }

    // Navigation
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
