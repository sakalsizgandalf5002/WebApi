using Api.Data;
using Api.Interfaces.IRepo;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repo;

public class RefreshTokenRepo : IRefreshTokenRepo
{
    private readonly AppDbContext _db;
    public RefreshTokenRepo(AppDbContext db)
    {
        _db = db;
    }
    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
    {
        return _db.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == token, ct);
    }

    public async Task CreateAsync(RefreshToken token, CancellationToken ct)
    {
        await _db.RefreshTokens.AddAsync(token, ct);
    }

    public Task UpdateAsync(RefreshToken token, CancellationToken ct)
    {
        _db.RefreshTokens.Update(token);
        return Task.CompletedTask;
    }
}