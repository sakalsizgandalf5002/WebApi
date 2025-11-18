using Api.Interfaces.IService;
using Api.Models;
using Api.Options;
using Microsoft.Extensions.Options;

namespace Api.Service;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly JwtOptions _jwt;

    private readonly IUnitOfWork _uow;
    private readonly ITokenService _tokenService;

    public RefreshTokenService(IOptions<JwtOptions> jwtOptions, IUnitOfWork uow, ITokenService tokenService)
    {
        _jwt = jwtOptions.Value;
        _uow = uow;
        _tokenService = tokenService;
    }

    public async Task<(string accessToken, RefreshToken refreshToken)> IssueTokensAsync(AppUser user, string ip, CancellationToken ct)
    {
        var accessToken = _tokenService.CreateToken(user);

        var refresh = NewRefreshToken(user.Id, ip);

        await _uow.RefreshTokens.CreateAsync(refresh, ct);
        await _uow.SaveChangesAsync(CancellationToken.None);

        return (accessToken, refresh);
    }

    public async Task<(string accessToken, RefreshToken refreshToken)> RotateAsync(string refreshToken, string ip, CancellationToken ct)
    {
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

    public async Task RevokeAsync(string refreshToken, string ip, string reason, CancellationToken ct)
    {
        var token = await _uow.RefreshTokens.GetByTokenAsync(refreshToken, ct);
        if (token == null || !token.IsActive)
            return;

        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ip;

        await _uow.RefreshTokens.UpdateAsync(token, ct);
        await _uow.SaveChangesAsync(ct);
    }

    private RefreshToken NewRefreshToken(string userId, string ip)
    {
        return new RefreshToken
        {
            UserId = userId,
            Token = Guid.NewGuid().ToString("N"),
            Expires = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ip
        };
    }
}    