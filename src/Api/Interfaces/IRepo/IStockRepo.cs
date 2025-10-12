using Api.Models;
public interface IStockRepo
{
    IQueryable<Stock> Query();
    Task<Stock?> GetByIdAsync(int id, CancellationToken ct);
    Task<Stock?> GetBySymbolAsync(string symbol, CancellationToken ct);

    Task CreateAsync(Stock stock, CancellationToken ct);  // geriye entity dönmene gerek yok
    Task UpdateAsync(Stock stock, CancellationToken ct);  // DTO değil, entity
    Task DeleteAsync(Stock stock, CancellationToken ct);  // id değil, entity
    Task<bool> StockExists(int id, CancellationToken ct);
}
