using MassTransit;
using MediatR;
using SocialApp.UserService.Domain.Repositories;
using EventBus.User;

namespace SocialApp.UserService.Application.Features.UpdateUser;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, UpdateUserResponse>
{
    private readonly IUserRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<UpdateUserHandler> _logger;

    public UpdateUserHandler(IUserRepository repository, IPublishEndpoint publishEndpoint, ILogger<UpdateUserHandler> logger)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<UpdateUserResponse> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByAuthUserIdAsync(command.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng");

        bool isEmailChanged = false;

        if (!string.IsNullOrWhiteSpace(command.Email) && command.Email != user.Email)
        {
            user.Email = command.Email;
            isEmailChanged = true;
        }

        // Cập nhật các trường nếu có giá trị
        if (command.FirstName is not null) user.FirstName = command.FirstName;
        if (command.LastName is not null) user.LastName = command.LastName;
        if (command.Phone is not null) user.Phone = command.Phone;
        if (command.Avatar is not null) user.Avatar = command.Avatar;
        if (command.Dob is not null) user.Dob = command.Dob;
        if (command.Bio is not null) user.Bio = command.Bio;
        if (command.Location is not null) user.Location = command.Location;
        if (command.Website is not null) user.Website = command.Website;

        user.UpdatedAt = DateTime.UtcNow;

        _repository.Update(user);
        await _repository.SaveChangesAsync(cancellationToken);

        // Publish event
        try
        {
            if (isEmailChanged)
                await _publishEndpoint.Publish(new UserEmailUpdatedEvent
                (
                    user.AuthUserId,
                    user.Email
                ), cancellationToken);

            _logger.LogInformation(
                "User profile updated successfully. UserId: {UserId}", user.AuthUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish user updated event. UserId: {UserId}",
                user.Id);
        }
        return new UpdateUserResponse(
               user.Id, user.FirstName, user.LastName, user.Avatar, user.UpdatedAt
           );
    }
}
