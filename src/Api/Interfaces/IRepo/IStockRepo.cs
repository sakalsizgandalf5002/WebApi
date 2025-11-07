using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Api.Helpers;
using Api.Models;

namespace Api.Interfaces.IRepo;

public interface IStockRepo
{
    IQueryable<Stock> Query();
    Task<(IReadOnlyList<Stock> Items, int Total)> QueryAsync(QueryObject q, CancellationToken ct);

    Task<Stock?> GetByIdAsync(int id, CancellationToken ct);
    Task<Stock?> GetBySymbolAsync(string symbol, CancellationToken ct);

    Task CreateAsync(Stock stock, CancellationToken ct);
    Task UpdateAsync(Stock stock, CancellationToken ct);
    Task DeleteAsync(Stock stock, CancellationToken ct);
    Task<bool> StockExists(int id, CancellationToken ct);
}