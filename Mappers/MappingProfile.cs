using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.DTOs;
using Api.DTOs.Comment;
using Api.DTOs.Stock;
using Api.Models;
using AutoMapper;

namespace Api.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Stock, StockDto>()
            .ForMember(d => d.Comments, opt => opt.MapFrom(s => s.Comments));
            CreateMap<CreateStockRequestDto, Stock>();
            CreateMap<UpdateStockRequestDto, Stock>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Comment, CommentDto>()
            .ForMember(d => d.CreatedBy, opt => opt.MapFrom(s => s.AppUser != null ? s.AppUser.UserName : "unknown"))
            .ForMember(d => d.StockId, opt => opt.MapFrom(s => s.StockId));
            CreateMap<CreateCommentDto, Comment>();
            CreateMap<UpdateCommentRequestDto, Comment>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            
            CreateMap<AppUser, Api.DTOs.Account.NewUserDto>();
        }
    }
}