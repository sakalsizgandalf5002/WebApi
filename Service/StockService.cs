using Api.DTOs;
using Api.DTOs.Stock;
using Api.Helpers;
using Api.Interfaces;
using Api.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

public class StockService : IStockService
{
    private readonly IStockRepo _repo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public StockService(IStockRepo repo, IUnitOfWork uow, IMapper mapper)
    {
        _repo = repo;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<StockDto>>> QueryAsync(QueryObject q, CancellationToken ct)
    {
        var query = _repo.Query();

        if (!string.IsNullOrWhiteSpace(q.Symbol))
            query = query.Where(s => s.Symbol.Contains(q.Symbol));

        if (!string.IsNullOrWhiteSpace(q.CompanyName))
            query = query.Where(s => s.CompanyName.Contains(q.CompanyName));

        if (!string.IsNullOrWhiteSpace(q.SortBy))
        {
            if (q.SortBy.Equals("Symbol", StringComparison.OrdinalIgnoreCase))
                query = q.IsDescending ? query.OrderByDescending(s => s.Symbol) : query.OrderBy(s => s.Symbol);
            else if (q.SortBy.Equals("Purchase", StringComparison.OrdinalIgnoreCase))
                query = q.IsDescending ? query.OrderByDescending(s => s.Purchase) : query.OrderBy(s => s.Purchase);
        }

        var total = await query.CountAsync(ct);
        var skip = (q.PageNumber - 1) * q.PageSize;
        var items = await query.Skip(skip).Take(q.PageSize).ToListAsync(ct);

        var dtos = items.Select(_mapper.Map<StockDto>).ToList();
        return Result<PagedResult<StockDto>>.Ok(new PagedResult<StockDto>(dtos, total, q.PageNumber, q.PageSize));
    }

    public async Task<Result<StockDto>> CreateAsync(CreateStockRequestDto dto, string userId, CancellationToken ct)
    {
        var entity = _mapper.Map<Stock>(dto);
        await _repo.CreateAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<StockDto>.Ok(_mapper.Map<StockDto>(entity));
    }

    public async Task<Result<StockDto>> UpdateAsync(int id, UpdateStockRequestDto dto, string userId, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<StockDto>.Fail("not_found");

        // DTO->entity mapping serviste
        entity.Symbol = dto.Symbol;
        entity.CompanyName = dto.CompanyName;
        entity.Purchase = dto.Purchase;
        entity.LastDiv = dto.LastDiv;
        entity.Industry = dto.Industry;
        entity.MarketCap = dto.MarketCap;

        await _repo.UpdateAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<StockDto>.Ok(_mapper.Map<StockDto>(entity));
    }

    public async Task<Result<bool>> DeleteAsync(int id, string userId, CancellationToken ct)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return Result<bool>.Fail("not_found");

        await _repo.DeleteAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Ok(true);
    }
    public async Task<Result<StockDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        var entity = await _repo.Query()
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (entity is null)
            return Result<StockDto>.Fail("not_found");

        var dto = _mapper.Map<StockDto>(entity);
        return Result<StockDto>.Ok(dto);
    }
    public async Task<Result<StockDto>> GetBySymbolAsync(string symbol, CancellationToken ct)
{
    if (string.IsNullOrWhiteSpace(symbol))
        return Result<StockDto>.Fail("symbol_required");

    var entity = await _repo.Query()
        .FirstOrDefaultAsync(s => s.Symbol == symbol, ct);

    if (entity is null)
        return Result<StockDto>.Fail("not_found");

    var dto = _mapper.Map<StockDto>(entity);
    return Result<StockDto>.Ok(dto);
}
}
