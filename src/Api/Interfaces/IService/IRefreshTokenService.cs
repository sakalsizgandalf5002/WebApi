using Api.Models;

namespace Api.Interfaces.IService;

public interface IRefreshTokenService
{
    Task<(string accessToken, RefreshToken refreshToken)> IssueTokensAsync(AppUser user, string ip);
    Task<(string accessToken, RefreshToken refreshToken)> RotateAsync(string refreshToken, string ip);
    Task RevokeAsync(string refreshToken, string ip, string reason);
}