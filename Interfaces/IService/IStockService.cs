using Api.DTOs;
using Api.DTOs.Stock;
using Api.Helpers;

public interface IStockService
{
    Task<Result<PagedResult<StockDto>>> QueryAsync(QueryObject query, CancellationToken ct);
    Task<Result<StockDto>> GetByIdAsync(int id, CancellationToken ct);
    Task<Result<StockDto>> GetBySymbolAsync(string symbol, CancellationToken ct);
    Task<Result<StockDto>> CreateAsync(CreateStockRequestDto dto, string userId, CancellationToken ct);
    Task<Result<StockDto>> UpdateAsync(int id, UpdateStockRequestDto dto, string userId, CancellationToken ct);
    Task<Result<bool>> DeleteAsync(int id, string userId, CancellationToken ct);
}
