using Api.Data;
using Api.Helpers;
using Api.Interfaces.IRepo;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repo;

public class StockRepo : IStockRepo
{
    private readonly AppDbContext _context;
    private readonly ILogger<StockRepo> _logger;

    public StockRepo(AppDbContext context, ILogger<StockRepo> logger)
    {
        _context = context;
        _logger = logger;
    }

    public IQueryable<Stock> Query() => _context.Stocks.AsQueryable();

    public async Task<(IReadOnlyList<Stock> Items, int Total)> QueryAsync(QueryObject q, CancellationToken ct)
    {
        var query = _context.Stocks.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q.Symbol))
            query = query.Where(s => s.Symbol.Contains(q.Symbol));

        if (!string.IsNullOrWhiteSpace(q.CompanyName))
            query = query.Where(s => s.CompanyName.Contains(q.CompanyName));

        if (!string.IsNullOrWhiteSpace(q.SortBy))
        {
            if (q.SortBy.Equals("Symbol", System.StringComparison.OrdinalIgnoreCase))
                query = q.IsDescending ? query.OrderByDescending(s => s.Symbol).ThenBy(s => s.Id)
                                       : query.OrderBy(s => s.Symbol).ThenBy(s => s.Id);
            else if (q.SortBy.Equals("Purchase", System.StringComparison.OrdinalIgnoreCase))
                query = q.IsDescending ? query.OrderByDescending(s => s.Purchase).ThenBy(s => s.Id)
                                       : query.OrderBy(s => s.Purchase).ThenBy(s => s.Id);
            else
                query = query.OrderBy(s => s.Id);
        }
        else
        {
            query = query.OrderBy(s => s.Id);
        }

        var total = await query.CountAsync(ct);
        var skip = (q.PageNumber - 1) * q.PageSize;
        var items = await query.Skip(skip).Take(q.PageSize).ToListAsync(ct);

        return (items, total);
    }

    public async Task<Stock?> GetByIdAsync(int id, CancellationToken ct)
    {
        var entity = await _context.Stocks.FirstOrDefaultAsync(s => s.Id == id, ct);
        _logger.LogDebug(entity is null
            ? "StockRepo.GetById Id:{Id} → null"
            : "StockRepo.GetById Id:{Id} → hit", id);
        return entity;
    }

    public async Task<Stock?> GetBySymbolAsync(string symbol, CancellationToken ct)
    {
        var entity = await _context.Stocks.FirstOrDefaultAsync(s => s.Symbol == symbol, ct);
        _logger.LogDebug(entity is null
            ? "StockRepo.GetBySymbol {Symbol} → null"
            : "StockRepo.GetBySymbol {Symbol} → hit", symbol);
        return entity;
    }

    public async Task CreateAsync(Stock stock, CancellationToken ct)
    {
        await _context.Stocks.AddAsync(stock, ct);
        _logger.LogDebug("StockRepo.Create prepared Symbol:{Symbol}", stock.Symbol);
    }

    public Task UpdateAsync(Stock stock, CancellationToken ct)
    {
        _context.Stocks.Update(stock);
        _logger.LogDebug("StockRepo.Update prepared Id:{Id}", stock.Id);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Stock stock, CancellationToken ct)
    {
        _context.Stocks.Remove(stock);
        _logger.LogDebug("StockRepo.Delete prepared Id:{Id}", stock.Id);
        return Task.CompletedTask;
    }

    public async Task<bool> StockExists(int id, CancellationToken ct)
    {
        var exists = await _context.Stocks.AnyAsync(s => s.Id == id, ct);
        _logger.LogDebug("StockRepo.StockExists Id:{Id} → {Exists}", id, exists);
        return exists;
    }
}