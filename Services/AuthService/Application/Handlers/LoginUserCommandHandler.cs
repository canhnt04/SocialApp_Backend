using MediatR;
using SocialApp.AuthService.Application.Commands;
using SocialApp.AuthService.Application.DTOs.Responses;
using SocialApp.AuthService.Application.Interfaces;
using SocialApp.AuthService.Domain.Entities;
using SocialApp.AuthService.Domain.Repositories;

namespace SocialApp.AuthService.Application.Handlers;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginResponse>
{
    private readonly IAuthRepository _repository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;

    public LoginUserCommandHandler(
        IAuthRepository repository,
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService)
    {
        _repository = repository;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByUsernameOrEmailAsync(request.UsernameOrEmail, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Tài khoản không tồn tại");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Tên đăng nhập hoặc mật khẩu không hợp lệ");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Tài khoản đã bị khóa");

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenValue = _tokenService.GenerateRefreshToken(user);
        var refreshExpiresAt = _tokenService.GetRefreshTokenExpiresAt();

        var refreshToken = new RefreshToken
        {
            Token = refreshTokenValue,
            ExpiresAt = refreshExpiresAt,
            UserId = user.Id
        };

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new LoginResponse(
            user.Id,
            user.Username,
            accessToken,
            refreshTokenValue,
            refreshExpiresAt);
    }
}
