using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.DTOs.Comment;
using Api.Helpers;
using Api.Interfaces;
using Api.Interfaces.IService;
using Api.Mappers;
using AutoMapper;

namespace Api.Service
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepo _repo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CommentService(ICommentRepo repo, IUnitOfWork uow, IMapper mapper)
        {
            _repo = repo;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<Result<List<CommentDto>>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            var dtoList = list.Select(_mapper.Map<CommentDto>).ToList();
            return Result<List<CommentDto>>.Ok(dtoList);
        }

        public async Task<Result<CommentDto>> GetByIdAsync(int id)
        {
            var e = await _repo.GetByIdAsync(id);
            if (e == null) return Result<CommentDto>.Fail("not_found");
            return Result<CommentDto>.Ok(_mapper.Map<CommentDto>(e));
        }

        public async Task<Result<CommentDto>> CreateAsync(CreateCommentDto dto, string userId, int stockId)
        {
            var entity = _mapper.Map<Models.Comment>(dto);
            entity.AppUserId = userId;
            entity.StockId = stockId;

            var created = await _repo.CreateAsync(entity);
            await _uow.SaveChangesAsync();

            return Result<CommentDto>.Ok(_mapper.Map<CommentDto>(created));
        }

        public async Task<Result<CommentDto>> UpdateAsync(int id, UpdateCommentRequestDto dto, string userId)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return Result<CommentDto>.Fail("not_found");
            if (existing.AppUserId != userId) return Result<CommentDto>.Fail("forbidden");

            _mapper.Map(dto, existing);

            var updated = await _repo.UpdateAsync(id, existing);
            await _uow.SaveChangesAsync();

            return Result<CommentDto>.Ok(_mapper.Map<CommentDto>(updated));
        }

        public async Task<Result<bool>> DeleteAsync(int id, string userId)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return Result<bool>.Fail("not_found");
            if (existing.AppUserId != userId) return Result<bool>.Fail("forbidden");

            var deleted = await _repo.DeleteAsync(id);
            await _uow.SaveChangesAsync();

            return Result<bool>.Ok(deleted != null);
        }
    }
}