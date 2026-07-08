namespace SocialApp.UserService.Infrastructure.Authentication;

public interface ICurrentUser
{
    Guid? Id { get; }
    string? Username { get; }
    bool IsAuthenticated { get; }
}
