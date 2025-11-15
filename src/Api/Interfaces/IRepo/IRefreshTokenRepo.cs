using Api.Models;

namespace Api.Interfaces.IRepo;

public interface IRefreshTokenRepo
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct);
    Task CreateAsync(RefreshToken token, CancellationToken ct);
    Task UpdateAsync(RefreshToken token, CancellationToken ct);
}