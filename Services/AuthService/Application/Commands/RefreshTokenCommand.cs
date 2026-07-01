using MediatR;
using SocialApp.AuthService.Application.DTOs;

namespace SocialApp.AuthService.Application.Commands;

public record RefreshTokenCommand(
    string RefreshToken
) : IRequest<AuthResponse>;
