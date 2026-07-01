using MediatR;
using SocialApp.AuthService.Application.DTOs;

namespace SocialApp.AuthService.Application.Commands;

public record LoginUserCommand(
    string UsernameOrEmail,
    string Password
) : IRequest<AuthResponse>;
