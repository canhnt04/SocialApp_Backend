using MediatR;

namespace SocialApp.UserService.Application.Features.UpdateUser;

public record UpdateUserCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Avatar,
    DateOnly? Dob,
    string? Bio,
    string? Location,
    string? Website
) : IRequest<UpdateUserResponse>;
