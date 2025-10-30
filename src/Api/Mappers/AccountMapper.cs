using AutoMapper;
using Api.Models;
using Api.DTOs.Account;
namespace Api.Mappers;

public class AccountMapper : Profile
{
    public AccountMapper()
    {
        CreateMap<AppUser, NewUserDto>()
            .ForMember(d => d.Token, o=> o.Ignore());

    }
}