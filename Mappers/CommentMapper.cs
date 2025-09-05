using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.DTOs.Comment;
using Api.Models;

namespace Api.Mappers
{
    public static class CommentMapper
    {
        public static CommentDto ToCommentDto(this Comment commentModel)
            => new CommentDto
            {
                Id = commentModel.Id,
                Title = commentModel.Title,
                Body = commentModel.Body,
                StockId = commentModel.StockId,
                CreatedBy = commentModel.AppUser.UserName,
                CreatedAt = commentModel.CreatedAt
            };

        public static Comment ToCommentFromCreate(this CreateCommentDto commentDto, int stockId)
        => new Comment
        {
            Title = commentDto.Title,
            Body = commentDto.Body,
            StockId = stockId
        };
            
            public static Comment ToCommentFromUpdate(this UpdateCommentRequestDto commentDto)
            => new Comment
            {
                Title = commentDto.Title,
                Body = commentDto.Body
            };
    }
}
