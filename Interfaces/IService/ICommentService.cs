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
         Task<Result<List<CommentDto>>> GetAllAsync();
        Task<Result<CommentDto>> GetByIdAsync(int id);
        Task<Result<CommentDto>> CreateAsync(CreateCommentDto dto, string userId, int stockId);
        Task<Result<CommentDto>> UpdateAsync(int id, UpdateCommentRequestDto dto, string userId);
        Task<Result<bool>> DeleteAsync(int id, string userId);
    }
}