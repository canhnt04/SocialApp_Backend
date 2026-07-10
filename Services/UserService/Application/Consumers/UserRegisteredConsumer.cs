namespace SocialApp.UserService.Application.Consumers;

using MassTransit;
using EventBus.Auth;
using SocialApp.UserService.Domain.Repositories;

public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IUserRepository _repository;
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(IUserRepository repository, ILogger<UserRegisteredConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var message = context.Message;

        var isExistingUser = await _repository.GetByAuthUserIdAsync(message.UserId, context.CancellationToken);
        var userProfile = new Domain.Entities.UserProfile
        {
            Id = Guid.NewGuid(),
            AuthUserId = message.UserId,
            Username = message.Username,
            FirstName = string.Empty,
            LastName = string.Empty,
            Email = message.Email,
            Phone = message.Phone,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(userProfile, context.CancellationToken);
        await _repository.SaveChangesAsync(context.CancellationToken);
        _logger.LogInformation("User profile created for AuthUserId: {AuthUserId}", message.UserId);
    }
}
