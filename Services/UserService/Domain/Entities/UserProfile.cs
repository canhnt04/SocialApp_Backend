using SocialApp.UserService.Domain.Base;

namespace SocialApp.UserService.Domain.Entities;

public class UserProfile : BaseEntity
{
    /// <summary>
    /// ID người dùng từ AuthService (liên kết ngoài)
    /// </summary>
    public Guid AuthUserId { get; set; }

    public string Username { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string? Avatar { get; set; }
    public DateOnly? Dob { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string? Website { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastActiveAt { get; set; }
}
