using Api.Models;
namespace Api.Tests.Test.Infrastructure;

public class EntityHelper
{
    public static Stock Copy(Stock s) => new()
    {
        Id = s.Id,
        Symbol = s.Symbol,
        Purchase = s.Purchase,
        CompanyName = s.CompanyName,
        LastDiv = s.LastDiv,
        Industry = s.Industry,
        MarketCap = s.MarketCap,
    };
}