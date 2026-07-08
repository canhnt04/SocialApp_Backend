using MediatR;
using SocialApp.AuthService.Application.DTOs.Responses;

namespace SocialApp.AuthService.Application.Commands;

public record LoginUserCommand(
    string UsernameOrEmail,
    string Password
) : IRequest<LoginResponse>;
