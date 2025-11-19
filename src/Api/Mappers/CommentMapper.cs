using Api.DTOs.Comment;
using Api.Models;
using AutoMapper;

namespace Api.Mappers
{
    public class CommentMapper : Profile
    {
        public CommentMapper()
        {
            CreateMap<Comment, CommentDto>()
                .ForMember(d => d.CreatedBy,
                    opt => opt.MapFrom(s => s.AppUser != null ? s.AppUser.UserName : "unknown"))
                .ForMember(d => d.StockId,
                    opt => opt.MapFrom(s => s.StockId));

            CreateMap<CreateCommentDto, Comment>()
                .ForMember(d => d.Id,        o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.StockId,   o => o.Ignore())
                .ForMember(d => d.Stock,     o => o.Ignore())
                .ForMember(d => d.AppUser,   o => o.Ignore())
                .ForMember(d => d.AppUserId, o => o.Ignore());

            var upd = CreateMap<UpdateCommentRequestDto, Comment>();

            upd.ForAllMembers(o =>
                o.Condition((src, dest, srcMember) => srcMember != null));

            upd.ForMember(d => d.Id,        o => o.Ignore());
            upd.ForMember(d => d.CreatedAt, o => o.Ignore());
            upd.ForMember(d => d.StockId,   o => o.Ignore());
            upd.ForMember(d => d.Stock,     o => o.Ignore());
            upd.ForMember(d => d.AppUser,   o => o.Ignore());
            upd.ForMember(d => d.AppUserId, o => o.Ignore());
        }
    }
}