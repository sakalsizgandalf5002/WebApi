using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Data;
using Api.Interfaces;
using Api.Interfaces.IRepo;
using Microsoft.EntityFrameworkCore;
using Api.Models;

namespace Api.Repo
{
    public class PortfolioRepo : IPortfolioRepo
    {
        private readonly AppDbContext _context;
        public PortfolioRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(Portfolio portfolio, CancellationToken ct)
        {
            await _context.Portfolios.AddAsync(portfolio, ct);
        }

        public Task DeleteAsync(Portfolio portfolio, CancellationToken ct)
        {
             _context.Portfolios.Remove(portfolio);
            return Task.CompletedTask;
        }

        public async Task<Portfolio?> FindAsync(string userId, string symbol, CancellationToken ct)
        {
            return await _context.Portfolios
            .AsNoTracking()
            .Include(p => p.Stock) 
            .FirstOrDefaultAsync(p => p.AppUserId == userId && p.Stock.Symbol == symbol, ct);
        }

        public async Task<IReadOnlyList<Stock>> GetUserStockAsync(AppUser user, CancellationToken ct)
        {
            return await _context.Portfolios
           .AsNoTracking()
           .Where(p => p.AppUserId == user.Id)
           .Select(p => p.Stock)
           .ToListAsync(ct);
        }

    }
}