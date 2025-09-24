using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Data;
using Api.DTOs.Stock;
using Api.Helpers;
using Api.Interfaces;
using Api.Models;
using Microsoft.EntityFrameworkCore;


namespace Api.Repo
{
    public class StockRepo : IStockRepo
    {
        private readonly AppDbContext _context;
        public StockRepo(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Stock?> CreateAsync(Stock stockModel)
        {
            await _context.Stocks.AddAsync(stockModel);
            return stockModel;
        }

        public async Task<Stock?> DeleteAsync(int id)
        {
            var stockModel = await _context.Stocks.FirstOrDefaultAsync(s => s.Id == id);
            if (stockModel == null)
            {
                return null;
            }

            _context.Stocks.Remove(stockModel);
            return stockModel;
        }

        public async Task<List<Stock>> GetAllAsync(QueryObject query)
        {
            var stocks = _context.Stocks.Include(c => c.Comments).ThenInclude(a => a.AppUser).AsQueryable();
            if (!string.IsNullOrWhiteSpace(query.Symbol))
            {
                stocks = stocks.Where(s => s.Symbol.Contains(query.Symbol));
            }
            if (!string.IsNullOrWhiteSpace(query.CompanyName))
            {
                stocks = stocks.Where(s => s.CompanyName.Contains(query.CompanyName));
            }

            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                if (query.SortBy.Equals("Symbol", StringComparison.OrdinalIgnoreCase))
                {
                    stocks = query.IsDescending ? stocks.OrderByDescending(s => s.Symbol) : stocks.OrderBy(s => s.Symbol);
                }
            }

            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                if (query.SortBy.Equals("Purchase", StringComparison.OrdinalIgnoreCase))
                {
                    stocks = query.IsDescending ? stocks.OrderByDescending(s => s.Purchase) : stocks.OrderBy(s => s.Purchase);
                }
            }
            var skipNumber = (query.PageNumber - 1) * query.PageSize;

            return await stocks.Skip(skipNumber).Take(query.PageSize).ToListAsync();
        }
        public async Task<Stock?> GetByIdAsync(int id)
        {
            return await _context.Stocks
        .Include(s => s.Comments)
            .ThenInclude(c => c.AppUser)  
        .FirstOrDefaultAsync(s => s.Id == id);
        }
        public async Task<Stock?> UpdateAsync(int id, UpdateStockRequestDto stockDto)
        {
            var existingStock = await _context.Stocks.FirstOrDefaultAsync(s => s.Id == id);
            if (existingStock == null)
            {
                return null;
            }
            existingStock.Symbol = stockDto.Symbol;
            existingStock.CompanyName = stockDto.CompanyName;
            existingStock.Purchase = stockDto.Purchase;
            existingStock.LastDiv = stockDto.LastDiv;
            existingStock.Industry = stockDto.Industry;
            existingStock.MarketCap = stockDto.MarketCap;
            return existingStock;
        }
        public Task<bool> StockExists(int id)
        {
            return _context.Stocks.AnyAsync(s => s.Id == id);
        }
        public async Task<Stock?> GetBySymbolAsync(string symbol, CancellationToken ct)
        {
            return await _context.Stocks.FirstOrDefaultAsync(s => s.Symbol == symbol, ct);
        }

        public async Task<PagedResult<Stock>> QueryAsync(QueryObject query)
        {
            var q = _context.Stocks.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Symbol))
                q = q.Where(s => s.Symbol.Contains(query.Symbol));

            if (!string.IsNullOrWhiteSpace(query.Industry))
                q = q.Where(s => s.Industry.Contains(query.Industry));

            if (!string.IsNullOrWhiteSpace(query.CompanyName))
                q = q.Where(s => s.CompanyName.Contains(query.CompanyName));

            if (!string.IsNullOrWhiteSpace(query.SortBy))
                q = query.IsDescending ? q.OrderByDescending(x => EF.Property<object>(x, query.SortBy))
                                      : q.OrderBy(x => EF.Property<object>(x, query.SortBy));

            var total = await q.CountAsync();

            var page = query.PageNumber <= 0 ? 1 : query.PageNumber;
            var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;

            var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var result = new Api.Helpers.PagedResult<Stock>();
    result.Items = items;
    result.TotalCount = total;
    result.PageNumber = page;
    result.PageSize = pageSize;
    return result;


        }
    }
}