using Api.Models;
using Api.Mappers;
using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;

namespace Api.Tests.Test.Infrastructure;

public class AutoMapperFixture
{
    public IMapper Mapper { get; }

    public AutoMapperFixture()
    {
        var cfg = new MapperConfiguration(c =>
        {
            c.AddProfile<StockMapper>();
            c.AddProfile<CommentMapper>();
            c.AddProfile<AccountMapper>();
        }, NullLoggerFactory.Instance);
        cfg.AssertConfigurationIsValid();
        Mapper = cfg.CreateMapper();
    }
}