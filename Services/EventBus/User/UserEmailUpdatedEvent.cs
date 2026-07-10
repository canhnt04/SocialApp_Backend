namespace EventBus.User;

public record UserEmailUpdatedEvent
(
     Guid AuthUserId,
     string Email
);