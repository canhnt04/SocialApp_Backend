using MediatR;
using SocialApp.AuthService.Application.DTOs.Responses;

namespace SocialApp.AuthService.Application.Commands;

public record RegisterUserCommand(
    string Username,
    string Email,
    string Phone,
    string Password
) : IRequest<RegisterResponse>;
