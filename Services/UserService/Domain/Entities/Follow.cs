using SocialApp.Shared.Base;

namespace SocialApp.UserService.Domain.Entities;

public class Follow : BaseEntity
{
    public Guid FollowerId { get; set; }
    public Guid FollowingId { get; set; }
    public FollowStatus Status { get; set; } = FollowStatus.Pending;
    public bool IsActive { get; set; } = true;

    // Optional navigation properties if UserProfile is linked
    public UserProfile? Follower { get; set; }
    public UserProfile? Following { get; set; }
}

public enum FollowStatus
{
    Pending = 0,
    Accepted = 1,
    Declined = 2,
    Blocked = 3
}
