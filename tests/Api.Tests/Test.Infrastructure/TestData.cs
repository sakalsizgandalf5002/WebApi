using Api.Models;
using Api.DTOs.Stock;

namespace Api.Tests.Test.Infrastructure;

public static class TestData
{
    public static Stock Stock(
        int id = 1,
        string symbol = "AAPL",
        string companyName = "Apple",
        decimal purchase = 100)
        => new()
        {
            Id = id,
            Symbol = symbol,
            CompanyName = companyName,
            Purchase = purchase
        };

    public static CreateStockRequestDto CreateStockDto(
        string symbol = "AAPL",
        string companyName = "Apple",
        decimal purchase = 100)
        => new()
        {
            Symbol = symbol,
            CompanyName = companyName,
            Purchase = purchase
        };
    
    public static UpdateStockRequestDto UpdateStockDto(
        string symbol = "AAPL",
        string company = "Apple",
        decimal purchase = 100)
        => new()
        {
            Symbol = symbol,
            CompanyName = company,
            Purchase = purchase
        };
}