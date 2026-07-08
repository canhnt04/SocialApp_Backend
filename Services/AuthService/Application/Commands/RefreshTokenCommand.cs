using MediatR;
using SocialApp.AuthService.Application.DTOs.Responses;

namespace SocialApp.AuthService.Application.Commands;

public record RefreshTokenCommand(
    string RefreshToken
) : IRequest<RefreshTokenResponse>;
