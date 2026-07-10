using EventBus.User;
using MassTransit;
using SocialApp.AuthService.Domain.Repositories;

namespace SocialApp.AuthService.Application.Consumers;

public class UserEmailUpdatedConsumer : IConsumer<UserEmailUpdatedEvent>
{
    private readonly IAuthRepository _repository;
    private readonly ILogger<UserEmailUpdatedConsumer> _logger;

    public UserEmailUpdatedConsumer(IAuthRepository repository, ILogger<UserEmailUpdatedConsumer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserEmailUpdatedEvent> context)
    {
        var message = context.Message;

        var authUser = await _repository.GetByIdAsync(message.AuthUserId, context.CancellationToken);

        if (authUser == null)
        {
            _logger.LogWarning("Không tìm thấy tài khoản Auth cho ID: {AuthUserId}. Bỏ qua cập nhật Email.", message.AuthUserId);
            return;
        }

        authUser.Email = message.Email;
        authUser.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Đã đồng bộ Email mới thành công cho AuthUserId: {AuthUserId}", message.AuthUserId);
    }
}