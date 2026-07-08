using MediatR;
using SocialApp.AuthService.Application.Commands;
using SocialApp.AuthService.Application.Interfaces;
using SocialApp.AuthService.Domain.Entities;
using SocialApp.AuthService.Domain.Repositories;
using SocialApp.AuthService.Application.DTOs.Responses;

namespace SocialApp.AuthService.Application.Handlers;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IRefreshTokenRepository _repository;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(IRefreshTokenRepository repository, ITokenService tokenService)
    {
        _repository = repository;
        _tokenService = tokenService;
    }

    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenPrincipal = _tokenService.ValidateToken(request.RefreshToken, "RefreshToken", validateLifetime: false);
        if (tokenPrincipal == null)
            throw new UnauthorizedAccessException("Refresh token không hợp lệ");

        var existingToken = await _repository.GetByTokenAsync(request.RefreshToken, cancellationToken);

        if (existingToken == null)
            throw new UnauthorizedAccessException("Refresh token không hợp lệ");

        if (existingToken.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token đã hết hạn");

        var user = existingToken.User;

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshTokenValue = _tokenService.GenerateRefreshToken(user);
        var refreshExpiresAt = _tokenService.GetRefreshTokenExpiresAt();

        var newRefreshToken = new RefreshToken
        {
            Token = newRefreshTokenValue,
            ExpiresAt = refreshExpiresAt,
            UserId = user.Id
        };

        await _repository.AddAsync(newRefreshToken, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResponse(newAccessToken, newRefreshTokenValue, refreshExpiresAt);
    }
}
