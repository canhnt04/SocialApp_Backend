
public record UserRegisteredEvent
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public DateTime RegisteredAt { get; init; }
}