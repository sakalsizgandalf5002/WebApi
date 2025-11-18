using Api.Models;

namespace Api.Interfaces.IService;

public interface IRefreshTokenService
{
    Task<(string accessToken, RefreshToken refreshToken)> IssueTokensAsync(AppUser user, string ip, CancellationToken ct);
    Task<(string accessToken, RefreshToken refreshToken)> RotateAsync(string refreshToken, string ip, CancellationToken ct);
    Task RevokeAsync(string refreshToken, string ip, string reason, CancellationToken ct);
}