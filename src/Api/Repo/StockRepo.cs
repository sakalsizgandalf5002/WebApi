using Api.Data;
using Api.Interfaces;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

    public IQueryable<Stock> Query()
    {
        _logger.LogDebug("StockRepo.Query called → base IQueryable returned");
        return _context.Stocks.AsQueryable();
    }

    public async Task<Stock?> GetByIdAsync(int id, CancellationToken ct)
    {
        var entity = await _context.Stocks.FirstOrDefaultAsync(s => s.Id == id, ct);

        if (entity == null)
            _logger.LogDebug("StockRepo.GetById Id:{Id} → null result", id);
        else
            _logger.LogDebug("StockRepo.GetById Id:{Id} → entity hit", id);

        return entity;
    }

    public async Task<Stock?> GetBySymbolAsync(string symbol, CancellationToken ct)
    {
        var entity = await _context.Stocks.FirstOrDefaultAsync(s => s.Symbol == symbol, ct);

        if (entity == null)
            _logger.LogDebug("StockRepo.GetBySymbol Symbol:{Symbol} → null result", symbol);
        else
            _logger.LogDebug("StockRepo.GetBySymbol Symbol:{Symbol} → entity hit", symbol);

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
