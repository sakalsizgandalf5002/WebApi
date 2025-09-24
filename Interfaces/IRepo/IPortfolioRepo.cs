using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;

namespace Api.Interfaces
{
    public interface IPortfolioRepo
    {
        Task<IReadOnlyList<Stock>> GetUserStockAsync(AppUser user, CancellationToken ct);
        Task CreateAsync(Portfolio portfolio, CancellationToken ct);
        Task<Portfolio> FindAsync(string userId, string symbol, CancellationToken ct);
        Task DeleteAsync(Portfolio portfolio, CancellationToken ct);
    }
}