using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.DomainLogs;
using Api.DTOs.Stock;
using Api.Helpers;
using Api.Interfaces;
using Api.Interfaces.IRepo;
using Api.Interfaces.IService;
using Api.Models;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace Api.Service
{
    public class PortfolioService : IPortfolioService
    {
        private readonly IPortfolioRepo _portfolioRepo;
        private readonly IStockRepo _stockRepo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<PortfolioService> _logger;

        public PortfolioService(IPortfolioRepo portfolioRepo, IStockRepo stockRepo, IUnitOfWork uow, IMapper mapper, UserManager<AppUser> userManager, ILogger<PortfolioService> logger)
        {
            _portfolioRepo = portfolioRepo;
            _stockRepo = stockRepo;
            _uow = uow;
            _mapper = mapper;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyList<StockDto>>> GetUserPortfolioAsync(AppUser user, CancellationToken ct)
        {
            var stocks = await _portfolioRepo.GetUserStockAsync(user, ct);
            PortfolioLogs.PortfolioFetched(_logger, user.Id);
            var dto = stocks.Select(_mapper.Map<StockDto>).ToList().AsReadOnly();
            return Result<IReadOnlyList<StockDto>>.Ok(dto);
        }

        public async Task<Result<bool>> AddAsync(AppUser user, string symbol, CancellationToken ct)
        {
            var stock = await _stockRepo.GetBySymbolAsync(symbol, ct);
            if (stock == null)
            {
                PortfolioLogs.AddStockNotFound(_logger, symbol, user.Id);
                return Result<bool>.Fail("Stock not found.");
            }

            await _portfolioRepo.CreateAsync(new Portfolio { AppUserId = user.Id, StockId = stock.Id }, ct);
            await _uow.SaveChangesAsync(ct);
            PortfolioLogs.PortfolioStockAdded(_logger, symbol, user.Id);
            return Result<bool>.Ok(true);
        }
        public async Task<Result<bool>> RemoveAsync(AppUser user, string symbol, CancellationToken ct)
        {
            var existing = await _portfolioRepo.FindAsync(user.Id, symbol, ct);
            if (existing == null)
            {
                PortfolioLogs.RemoveNotFound(_logger, symbol, user.Id);
                return Result<bool>.Fail("Stock not found.");
            }

            await _portfolioRepo.DeleteAsync(existing, ct);
            await _uow.SaveChangesAsync(ct);
            PortfolioLogs.PortfolioStockRemoved(_logger, symbol, user.Id);
            return Result<bool>.Ok(true);
        }
    }
}