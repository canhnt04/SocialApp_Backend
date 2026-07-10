using EventBus.Auth;
using MassTransit;
using MediatR;
using SocialApp.AuthService.Application.Commands;
using SocialApp.AuthService.Application.DTOs.Responses;
using SocialApp.AuthService.Domain.Entities;
using SocialApp.AuthService.Domain.Repositories;

namespace SocialApp.AuthService.Application.Handlers;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterResponse>
{
    private readonly IAuthRepository _repository;
    private readonly IConfiguration _configuration;
    private readonly IPublishEndpoint _publishEndpoint;

    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        IAuthRepository repository,
        IConfiguration configuration,
        IPublishEndpoint publishEndpoint,
        ILogger<RegisterUserCommandHandler> logger
       )
    {
        _repository = repository;
        _configuration = configuration;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<RegisterResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // validate
        if (await _repository.ExistsByUsernameAsync(request.Username, cancellationToken))
            throw new InvalidOperationException("Tên đăng nhập đã tồn tại");
        if (await _repository.ExistsByEmailAsync(request.Email, cancellationToken))
            throw new InvalidOperationException("Email đã tồn tại");
        if (await _repository.ExistsByPhoneAsync(request.Phone, cancellationToken))
            throw new InvalidOperationException("Số điện thoại đã tồn tại");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            Phone = request.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        await _repository.AddUserAsync(user, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        // Publish event
        try
        {
            await _publishEndpoint.Publish(new UserRegisteredEvent
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Phone = user.Phone,
                RegisteredAt = DateTime.UtcNow
            }, cancellationToken);

            _logger.LogInformation(
                "User registered successfully. UserId: {UserId}, Username: {Username}",
                user.Id,
                user.Username);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish user registered event. UserId: {UserId}",
                user.Id);
        }



        return new RegisterResponse(
            user.Id,
            user.Username
        );
    }
}
