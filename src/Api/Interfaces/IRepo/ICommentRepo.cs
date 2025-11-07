using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Interfaces.IRepo
{
    public interface ICommentRepo
    {
        Task<List<Comment>> GetAllAsync(CancellationToken ct);
        Task<Comment?> GetByIdAsync(int id, CancellationToken ct);
        Task<Comment> CreateAsync(Comment commentModel, CancellationToken ct);
        Task<Comment?> UpdateAsync(int id, Comment commentModel, CancellationToken ct);
         Task<Comment?> DeleteAsync(int id, CancellationToken ct);
    }
}