using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.DTOs;
using Api.DTOs.Stock;
using Api.Helpers;

namespace Api.Interfaces.IService
{
    public interface IStockService
    {
    Task<Result<StockDto>> GetByIdAsync(int id);
    Task<Result<StockDto>> GetBySymbolAsync(string symbol);
    Task<Result<PagedResult<StockDto>>> QueryAsync(QueryObject query);
    Task<Result<StockDto>> CreateAsync(CreateStockRequestDto dto, string userId);
    Task<Result<StockDto>> UpdateAsync(int id, UpdateStockRequestDto dto, string userId);
    Task<Result<bool>> DeleteAsync(int id, string userId);
    }
}