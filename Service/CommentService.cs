using System;
using Api.DTOs.Comment;
using Api.Helpers;
using Api.Interfaces;
using Api.Models;
using Api.DomainLogs;
using AutoMapper;
using Api.Interfaces.IService;

namespace Api.Service
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepo _repo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ILogger<CommentService> _logger;

        public CommentService(ICommentRepo repo, IUnitOfWork uow, IMapper mapper, ILogger<CommentService> logger)
        {
            _repo = repo;
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<CommentDto>>> GetAllAsync(CancellationToken ct)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var list = await _repo.GetAllAsync(ct);
            sw.Stop();

            CommentLogs.CommentList(_logger, list.Count, sw.Elapsed.TotalMilliseconds);

            var dtoList = list.Select(_mapper.Map<CommentDto>).ToList();
            return Result<List<CommentDto>>.Ok(dtoList);
        }

        public async Task<Result<CommentDto>> GetByIdAsync(int id, CancellationToken ct)
        {
            var e = await _repo.GetByIdAsync(id, ct);
            if (e is null)
            {
                CommentLogs.CommentNotFound(_logger, id);
                return Result<CommentDto>.Fail("not_found");
            }

            return Result<CommentDto>.Ok(_mapper.Map<CommentDto>(e));
        }

        public async Task<Result<CommentDto>> CreateAsync(CreateCommentDto dto, string userId, int stockId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<CommentDto>.Fail("unauthorized");

            var entity = _mapper.Map<Models.Comment>(dto);
            entity.AppUserId = userId;
            entity.StockId = stockId;

            var created = await _repo.CreateAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            CommentLogs.CommentCreated(_logger, created.Id, userId);

            return Result<CommentDto>.Ok(_mapper.Map<CommentDto>(created));
        }

        public async Task<Result<CommentDto>> UpdateAsync(int id, UpdateCommentRequestDto dto, string userId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<CommentDto>.Fail("unauthorized");

            var existing = await _repo.GetByIdAsync(id, ct);
            if (existing is null)
            {
                CommentLogs.CommentNotFound(_logger, id);
                return Result<CommentDto>.Fail("not_found");
            }

            if (!string.Equals(existing.AppUserId, userId, StringComparison.Ordinal))
            {
                CommentLogs.CommentForbidden(_logger, id, userId ?? "anon");
                return Result<CommentDto>.Fail("forbidden");
            }

            _mapper.Map(dto, existing);

            // Repo UpdateAsync kullanıyorsan çağır ama dönüşünü umursama; tracked entity zaten güncel.
            // await _repo.UpdateAsync(id, existing, ct);

            await _uow.SaveChangesAsync(ct);

            CommentLogs.CommentUpdated(_logger, existing.Id, userId);
            return Result<CommentDto>.Ok(_mapper.Map<CommentDto>(existing));
        }

        public async Task<Result<bool>> DeleteAsync(int id, string userId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<bool>.Fail("unauthorized");

            var existing = await _repo.GetByIdAsync(id, ct);
            if (existing is null)
            {
                CommentLogs.CommentNotFound(_logger, id);
                return Result<bool>.Fail("not_found");
            }

            if (!string.Equals(existing.AppUserId, userId, StringComparison.Ordinal))
            {
                CommentLogs.CommentForbidden(_logger, id, userId ?? "anon");
                return Result<bool>.Fail("forbidden");
            }

            var deleted = await _repo.DeleteAsync(id, ct);
            await _uow.SaveChangesAsync(ct);

            CommentLogs.CommentDeleted(_logger, id, userId ?? "anon");
            return Result<bool>.Ok(deleted is not null);
        }
    }
}
