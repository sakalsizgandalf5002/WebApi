using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Interfaces;
using Api.Interfaces.IService;
using Api.Helpers;
using Api.DTOs.Stock;
using Api.DTOs;
using Api.Mappers;
using AutoMapper;
using Api.Models;

namespace Api.Service
{
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


        public async Task<Result<StockDto>> GetByIdAsync(int id)
        {
            var e = await _repo.GetByIdAsync(id);
            if (e == null) return Result<StockDto>.Fail("not_found");
            return Result<StockDto>.Ok(_mapper.Map<StockDto>(e));
        }

        public async Task<Result<StockDto>> GetBySymbolAsync(string symbol)
        {
            var e = await _repo.GetBySymbolAsync(symbol);
            if (e == null) return Result<StockDto>.Fail("not_found");
            return Result<StockDto>.Ok(_mapper.Map<StockDto>(e));
        }

        public async Task<Result<PagedResult<StockDto>>> QueryAsync(QueryObject query)
        {
            var page = await _repo.QueryAsync(query);
            var items = page.Items.Select(_mapper.Map<StockDto>).ToList();

            return Result<PagedResult<StockDto>>.Ok(new PagedResult<StockDto>
            {
                Items = items,
                TotalCount = page.TotalCount,
                PageNumber = page.PageNumber,
                PageSize = page.PageSize
            });
        }

        public async Task<Result<StockDto>> CreateAsync(CreateStockRequestDto dto, string userId)
        {
            var entity = _mapper.Map<Models.Stock>(dto);
            var created = await _repo.CreateAsync(entity);
            await _uow.SaveChangesAsync();
            return Result<StockDto>.Ok(_mapper.Map<StockDto>(created));
        }

        public async Task<Result<StockDto>> UpdateAsync(int id, UpdateStockRequestDto dto, string userId)
        {
            var e = await _repo.GetByIdAsync(id);
            if (e == null) return Result<StockDto>.Fail("not_found");

            _mapper.Map(dto, e);
            await _uow.SaveChangesAsync();

            return Result<StockDto>.Ok(_mapper.Map<StockDto>(e));
        }

        public async Task<Result<bool>> DeleteAsync(int id, string userId)
        {
            var deleted = await _repo.DeleteAsync(id);
            await _uow.SaveChangesAsync();
            return Result<bool>.Ok(deleted != null);
        }
    }
}