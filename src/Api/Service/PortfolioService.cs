
using Api.DTOs.Stock;
using Api.Helpers;
using Api.Interfaces.IRepo;
using Api.Interfaces.IService;
using Api.Models;
using AutoMapper;

namespace Api.Service
{
    public class PortfolioService : IPortfolioService
    {
        private readonly IPortfolioRepo _portfolioRepo;
        private readonly IStockRepo _stockRepo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ILogger<PortfolioService> _logger;

        public PortfolioService(
            IPortfolioRepo portfolioRepo,
            IStockRepo stockRepo,
            IUnitOfWork uow,
            IMapper mapper,
            ILogger<PortfolioService> logger)
        {
            _portfolioRepo = portfolioRepo;
            _stockRepo = stockRepo;
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyList<StockDto>>> GetUserPortfolioAsync(string? userId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<IReadOnlyList<StockDto>>.Fail("unauthorized");

            var stocks = await _portfolioRepo.GetUserStockAsync(userId, ct);

            PortfolioLogs.PortfolioFetched(_logger, userId);

            var dto = stocks
                .Select(_mapper.Map<StockDto>)
                .ToList()
                .AsReadOnly();

            return Result<IReadOnlyList<StockDto>>.Ok(dto);
        }

        public async Task<Result<bool>> AddAsync(string? userId, string symbol, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<bool>.Fail("unauthorized");

            var stock = await _stockRepo.GetBySymbolAsync(symbol, ct);
            if (stock is null)
            {
                PortfolioLogs.AddStockNotFound(_logger, symbol, userId);
                return Result<bool>.Fail("not_found");
            }

            var existing = await _portfolioRepo.FindAsync(userId, symbol, ct);
            if (existing is not null)
            {
                PortfolioLogs.AddDuplicate(_logger, symbol, userId);
                return Result<bool>.Fail("already_exists");
            }

            await _portfolioRepo.CreateAsync(
                new Portfolio
                {
                    AppUserId = userId,
                    StockId = stock.Id
                },
                ct);

            await _uow.SaveChangesAsync(ct);

            PortfolioLogs.PortfolioStockAdded(_logger, symbol, userId);
            return Result<bool>.Ok(true);
        }

        public async Task<Result<bool>> RemoveAsync(string? userId, string symbol, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<bool>.Fail("unauthorized");

            var existing = await _portfolioRepo.FindAsync(userId, symbol, ct);
            if (existing is null)
            {
                PortfolioLogs.RemoveNotFound(_logger, symbol, userId);
                return Result<bool>.Fail("not_found");
            }

            await _portfolioRepo.DeleteAsync(existing, ct);
            await _uow.SaveChangesAsync(ct);

            PortfolioLogs.PortfolioStockRemoved(_logger, symbol, userId);
            return Result<bool>.Ok(true);
        }
    }
}