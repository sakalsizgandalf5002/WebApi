using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;

namespace Api.Interfaces.IRepo
{
    public interface IPortfolioRepo
    {
        Task<IReadOnlyList<Stock>> GetUserStockAsync(string userId, CancellationToken ct);
        Task CreateAsync(Portfolio portfolio, CancellationToken ct);
        Task<Portfolio?> FindAsync(string userId, string symbol, CancellationToken ct);
        Task DeleteAsync(Portfolio portfolio, CancellationToken ct);
    }
}