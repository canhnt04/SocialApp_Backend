using MediatR;
using SocialApp.UserService.Application.Commands;
using SocialApp.UserService.Application.DTOs;
using SocialApp.UserService.Domain.Repositories;

namespace SocialApp.UserService.Application.Handlers;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserProfileDto>
{
    private readonly IUserRepository _repository;

    public UpdateUserCommandHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserProfileDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng");

        // Cập nhật các trường nếu có giá trị
        if (request.Data.FirstName is not null) user.FirstName = request.Data.FirstName;
        if (request.Data.LastName is not null) user.LastName = request.Data.LastName;
        if (request.Data.Phone is not null) user.Phone = request.Data.Phone;
        if (request.Data.Avatar is not null) user.Avatar = request.Data.Avatar;
        if (request.Data.Dob is not null) user.Dob = request.Data.Dob;
        if (request.Data.Bio is not null) user.Bio = request.Data.Bio;
        if (request.Data.Location is not null) user.Location = request.Data.Location;
        if (request.Data.Website is not null) user.Website = request.Data.Website;

        user.UpdatedAt = DateTime.UtcNow;

        _repository.Update(user);
        await _repository.SaveChangesAsync(cancellationToken);

        return new UserProfileDto(
            user.Id, user.AuthUserId, user.Username, user.FirstName, user.LastName,
            user.Email, user.Phone, user.Avatar, user.Dob, user.Bio,
            user.Location, user.Website, user.IsActive, user.LastActiveAt,
            user.CreatedAt, user.UpdatedAt
        );
    }
}
