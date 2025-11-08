using System.Linq;
using Api.Data;
using Api.Models;

namespace Api.IntegrationTests.Infra.Db;

public static class TestSeed
{
    public static void SeedStocks(AppDbContext db)
    {
        if (db.Stocks.Any()) return;

        db.Stocks.AddRange(
            new Stock { Symbol = "AAPL", CompanyName = "Apple",     Purchase = 100 },
            new Stock { Symbol = "MSFT", CompanyName = "Microsoft", Purchase = 200 },
            new Stock { Symbol = "NVDA", CompanyName = "NVIDIA",    Purchase = 300 },
            new Stock { Symbol = "AMZN", CompanyName = "Amazon",    Purchase = 150 },
            new Stock { Symbol = "TSLA", CompanyName = "Tesla",     Purchase = 250 }
        );
        db.SaveChanges();
    }
}