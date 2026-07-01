using MediatR;
using SocialApp.AuthService.Application.DTOs;

namespace SocialApp.AuthService.Application.Commands;

public record RegisterUserCommand(
    string Username,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateTime Dob,
    string Password
) : IRequest<AuthResponse>;
