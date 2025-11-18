using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.DTOs.Comment;
using Api.Helpers;

namespace Api.Interfaces.IService
{
    public interface ICommentService
    {
         Task<Result<List<CommentDto>>> GetAllAsync(CancellationToken ct);
        Task<Result<CommentDto>> GetByIdAsync(int id, CancellationToken ct);
        Task<Result<CommentDto>> CreateAsync(CreateCommentDto dto, string? userId, int stockId, CancellationToken ct);
        Task<Result<CommentDto>> UpdateAsync(int id, UpdateCommentRequestDto dto, string? userId, CancellationToken ct);
        Task<Result<bool>> DeleteAsync(int id, string? userId, CancellationToken ct);
    }
}