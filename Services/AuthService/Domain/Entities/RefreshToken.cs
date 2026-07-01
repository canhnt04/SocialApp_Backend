using SocialApp.Shared.Base;

namespace SocialApp.AuthService.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; }

    // Foreign key
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
}
