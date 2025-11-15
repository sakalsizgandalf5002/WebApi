using System.Threading;
using System.Threading.Tasks;
using Api.Interfaces.IRepo;

namespace Api.Interfaces.IService;

public interface IUnitOfWork
{
    IStockRepo Stocks { get; }
    ICommentRepo Comments { get; }
    IPortfolioRepo Portfolios { get; }
    IRefreshTokenRepo RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}