using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.DTOs.Stock;
using Api.Helpers;
using Api.Models;

namespace Api.Interfaces.IService
{
    public interface IPortfolioService
    {
        Task<Result<IReadOnlyList<StockDto>>> GetUserPortfolioAsync(string? userId, CancellationToken ct);
        Task<Result<bool>> AddAsync(string? userId, string symbol, CancellationToken ct);
        Task<Result<bool>> RemoveAsync(string? userId, string symbol, CancellationToken ct);
    }
}