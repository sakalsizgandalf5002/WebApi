using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Data;
using Api.Interfaces;
using Api.Interfaces.IRepo;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Repo
{
    public class CommentRepo : ICommentRepo
    {
        private readonly AppDbContext _context;

        public CommentRepo(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<Comment>> GetAllAsync(CancellationToken ct)
        {
            return await _context.Comments.Include(a => a.AppUser).ToListAsync(ct);
        }
        public async Task<Comment?> GetByIdAsync(int id, CancellationToken ct)
        {
            return await _context.Comments.Include(a => a.AppUser).FirstOrDefaultAsync(c => c.Id == id, ct);
        }
        public async Task<Comment> CreateAsync(Comment commentModel, CancellationToken ct)
        {
            await _context.Comments.AddAsync(commentModel, ct);
            return commentModel;
        }
        public async Task<Comment?> UpdateAsync(int id, Comment commentModel, CancellationToken ct)
        {
            var existingComment = await _context.Comments.FindAsync(id, ct);
            if (existingComment == null)
            {
                return null;
            }

            existingComment.Title = commentModel.Title;
            existingComment.Body = commentModel.Body;


            return existingComment;
        }
        public async Task<Comment?> DeleteAsync(int id, CancellationToken ct)
        {
            var commentModel = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (commentModel == null)
            {
                return null;
            }
            _context.Comments.Remove(commentModel);
            return commentModel;
        }
    }
}