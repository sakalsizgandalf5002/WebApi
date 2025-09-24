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
        Task<Result<IReadOnlyList<StockDto>>> GetUserPortfolioAsync(AppUser user, CancellationToken ct);
        Task<Result<bool>> AddAsync(AppUser user, string symbol, CancellationToken ct);
        Task<Result<bool>> RemoveAsync(AppUser user, string symbol, CancellationToken ct);
    }
}