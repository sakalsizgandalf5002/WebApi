using System;
using System.Threading;
using System.Threading.Tasks;
using Api.Interfaces;
using Api.Interfaces.IService;
using Api.Models;

namespace Api.Service;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IUnitOfWork _uow;
    private readonly ITokenService _tokenService;

    public RefreshTokenService(IUnitOfWork uow, ITokenService tokenService)
    {
        _uow = uow;
        _tokenService = tokenService;
    }

    public async Task<(string accessToken, RefreshToken refreshToken)> IssueTokensAsync(AppUser user, string ip)
    {
        var accessToken = _tokenService.CreateToken(user);

        var refresh = NewRefreshToken(user.Id, ip);

        await _uow.RefreshTokens.CreateAsync(refresh, CancellationToken.None);
        await _uow.SaveChangesAsync(CancellationToken.None);

        return (accessToken, refresh);
    }

    public async Task<(string accessToken, RefreshToken refreshToken)> RotateAsync(string refreshToken, string ip)
    {
        var ct = CancellationToken.None;

        var token = await _uow.RefreshTokens.GetByTokenAsync(refreshToken, ct);
        if (token == null || !token.IsActive)
            throw new InvalidOperationException("Invalid refresh token");

        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ip;

        var newToken = NewRefreshToken(token.UserId, ip);
        token.ReplacedByToken = newToken.Token;

        await _uow.RefreshTokens.CreateAsync(newToken, ct);
        await _uow.RefreshTokens.UpdateAsync(token, ct);

        var accessToken = _tokenService.CreateToken(token.User);
        await _uow.SaveChangesAsync(ct);

        return (accessToken, newToken);
    }

    public async Task RevokeAsync(string refreshToken, string ip, string reason)
    {
        var ct = CancellationToken.None;

        var token = await _uow.RefreshTokens.GetByTokenAsync(refreshToken, ct);
        if (token == null || !token.IsActive)
            return;

        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ip;

        await _uow.RefreshTokens.UpdateAsync(token, ct);
        await _uow.SaveChangesAsync(ct);
    }

    private static RefreshToken NewRefreshToken(string userId, string ip)
    {
        return new RefreshToken
        {
            UserId = userId,
            Token = Guid.NewGuid().ToString("N"),
            Expires = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ip
        };
    }
}