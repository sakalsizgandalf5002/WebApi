using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Models;

public class StockRepo : IStockRepo
{
    private readonly AppDbContext _context;
    public StockRepo(AppDbContext context) => _context = context;

    public IQueryable<Stock> Query() =>
        _context.Stocks
            .Include(s => s.Comments).ThenInclude(c => c.AppUser)
            .AsNoTracking();

    public Task<Stock?> GetByIdAsync(int id, CancellationToken ct) =>
        _context.Stocks.FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<Stock?> GetBySymbolAsync(string symbol, CancellationToken ct) =>
        _context.Stocks.FirstOrDefaultAsync(s => s.Symbol == symbol, ct);

    public Task CreateAsync(Stock stock, CancellationToken ct) =>
        _context.Stocks.AddAsync(stock, ct).AsTask();

    public Task UpdateAsync(Stock stock, CancellationToken ct)
    {
        _context.Stocks.Update(stock);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Stock stock, CancellationToken ct)
    {
        _context.Stocks.Remove(stock);
        return Task.CompletedTask;
    }

    public Task<bool> StockExists(int id, CancellationToken ct) =>
        _context.Stocks.AnyAsync(s => s.Id == id, ct);
}
