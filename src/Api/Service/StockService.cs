using Api.DTOs;
using Api.DTOs.Stock;
using Api.Helpers;
using Api.Interfaces;
using Api.Models;
using Api.DomainLogs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class StockService : IStockService
{
    private readonly IStockRepo _repo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<StockService> _logger;

    public StockService(IStockRepo repo, IUnitOfWork uow, IMapper mapper, ILogger<StockService> logger)
    {
        _repo = repo;
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PagedResult<StockDto>>> QueryAsync(QueryObject q, CancellationToken ct)
    {
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = "StockQuery",
            ["Page"] = q.PageNumber,
            ["Size"] = q.PageSize,
            ["SortBy"] = q.SortBy ?? "",
            ["Desc"] = q.IsDescending,
            ["Symbol"] = q.Symbol ?? "",
            ["ComnpanyName"] = q.CompanyName ?? ""
        }))
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var query = _repo.Query();
            query = query.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q.Symbol))
                query = query.Where(s => s.Symbol.Contains(q.Symbol));

            if (!string.IsNullOrWhiteSpace(q.CompanyName))
                query = query.Where(s => s.CompanyName.Contains(q.CompanyName));

            if (!string.IsNullOrWhiteSpace(q.SortBy))
            {
                if (q.SortBy.Equals("Symbol", StringComparison.OrdinalIgnoreCase))
                {
                    query = q.IsDescending
                        ? query.OrderByDescending(s => s.Symbol).ThenBy(s => s.Id)
                        : query.OrderBy(s => s.Symbol).ThenBy(s => s.Id);
                }
                else if (q.SortBy.Equals("Purchase", StringComparison.OrdinalIgnoreCase))
                {
                    query = q.IsDescending
                        ? query.OrderByDescending(s => s.Purchase).ThenBy(s => s.Id)
                        : query.OrderBy(s => s.Purchase).ThenBy(s => s.Id);
                }
                else
                {
                    query = query.OrderBy(s => s.Id);
                }
            }

            var total = await query.CountAsync(ct);
            var skip = (q.PageNumber - 1) * q.PageSize;
            var items = await query.Skip(skip).Take(q.PageSize).ToListAsync(ct);

            sw.Stop();
            StockLogs.StockQueryExecuted(_logger, total, sw.Elapsed.TotalMilliseconds);

            var dtos = items.Select(_mapper.Map<StockDto>).ToList();
            return Result<PagedResult<StockDto>>.Ok(new PagedResult<StockDto>(dtos, total, q.PageNumber, q.PageSize));
        }
    }

    public async Task<Result<StockDto>> CreateAsync(CreateStockRequestDto dto, string userId, CancellationToken ct)
    {
        var entity = _mapper.Map<Stock>(dto);
        await _repo.CreateAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        StockLogs.StockCreated(_logger, entity.Symbol, userId);

        return Result<StockDto>.Ok(_mapper.Map<StockDto>(entity));
    }

    public async Task<Result<StockDto>> UpdateAsync(int id, UpdateStockRequestDto dto, string userId, CancellationToken ct)
    {
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = "StockUpdate",
            ["Id"] = id,
            ["UserId"] = userId ?? "anon"
        }))
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity is null)
            {
                StockLogs.StockNotFound(_logger, id);
                return Result<StockDto>.Fail("not_found");
            }


            entity.Symbol = dto.Symbol;
            entity.CompanyName = dto.CompanyName;
            entity.Purchase = dto.Purchase;
            entity.LastDiv = dto.LastDiv;
            entity.Industry = dto.Industry;
            entity.MarketCap = dto.MarketCap;

            await _repo.UpdateAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            StockLogs.StockUpdated(_logger, entity.Id, userId?? "anon");

            return Result<StockDto>.Ok(_mapper.Map<StockDto>(entity));
        }
    }

    public async Task<Result<bool>> DeleteAsync(int id, string userId, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null)
        {
            StockLogs.StockNotFound(_logger, id);
            return Result<bool>.Fail("not_found");
        }

        await _repo.DeleteAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        StockLogs.StockDeleted(_logger, entity.Id, userId);
        return Result<bool>.Ok(true);
    }
    public async Task<Result<StockDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        var entity = await _repo.Query()
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (entity is null)
        {
            StockLogs.StockNotFound(_logger, id);
            return Result<StockDto>.Fail("not_found");
        }
        var dto = _mapper.Map<StockDto>(entity);
        return Result<StockDto>.Ok(dto);
    }
    public async Task<Result<StockDto>> GetBySymbolAsync(string symbol, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            StockLogs.StockSymbolEmpty(_logger);
            return Result<StockDto>.Fail("symbol_required");

        }
        var entity = await _repo.Query()
            .FirstOrDefaultAsync(s => s.Symbol == symbol, ct);

        if (entity is null)
        {
            StockLogs.StockSymbolNotFound(_logger, symbol);
            return Result<StockDto>.Fail("not_found");
        }
        var dto = _mapper.Map<StockDto>(entity);
        return Result<StockDto>.Ok(dto);
    }
}