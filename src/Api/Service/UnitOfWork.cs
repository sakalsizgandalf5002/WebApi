using System.Threading;
using System.Threading.Tasks;
using Api.Data;
using Api.Interfaces;
using Api.Interfaces.IRepo;
using Api.Interfaces.IService;

namespace Api.Service;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    public IStockRepo Stocks { get; }
    public ICommentRepo Comments { get; }
    public IPortfolioRepo Portfolios { get; }
    public IRefreshTokenRepo RefreshTokens { get; }

    public UnitOfWork(
        AppDbContext db,
        IStockRepo stocks,
        ICommentRepo comments,
        IPortfolioRepo portfolios,
        IRefreshTokenRepo refreshTokens)
    {
        _db = db;
        Stocks = stocks;
        Comments = comments;
        Portfolios = portfolios;
        RefreshTokens = refreshTokens;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return _db.SaveChangesAsync(ct);
    }
}