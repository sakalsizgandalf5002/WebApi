using Api.DTOs;
using AutoMapper;
using Api.DTOs.Stock;
using Api.Models;

namespace Api.Mappers;

public class StockMapper : Profile
{
    public StockMapper()
    {
        CreateMap<Stock, StockDto>()
            .ForMember(d => d.Comments, opt => opt.MapFrom(s => s.Comments));
        
        CreateMap<CreateStockRequestDto, Stock>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Comments, opt => opt.Ignore())
            .ForMember(d => d.Portfolios, opt => opt.Ignore());
        
        var upd = CreateMap<UpdateStockRequestDto, Stock>();
        upd.ForMember(d => d.Id,         o => o.Ignore());
        upd.ForMember(d => d.Comments,   o => o.Ignore());
        upd.ForMember(d => d.Portfolios, o => o.Ignore());
        upd.ForAllMembers(o => o.Condition((src, dest, srcMember) => srcMember != null));

    }
}