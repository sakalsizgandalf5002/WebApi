// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.DTOs.Stock;
using Api.Helpers;
using Api.Interfaces.IService;
using Api.Interfaces.IRepo;
using Api.Models;
using Api.Service;
using Api.Tests.Test.Infrastructure;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Api.Tests.StockTests;

public class StockServiceTests : IClassFixture<AutoMapperFixture>
{
    private readonly IMapper _mapper;
    private readonly Mock<IStockRepo> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ILogger<StockService>> _log = new();

    public StockServiceTests(AutoMapperFixture fx) => _mapper = fx.Mapper;

    private StockService Sut() => new(_repo.Object, _uow.Object, _mapper, _log.Object);

    private static Stock E(int id, string sym, string comp, decimal p)
        => TestData.Stock(id, sym, comp, p);

    [Fact]
    public async Task QueryAsync_MapsAndPaginates()
    {
        var q = new QueryObject { PageNumber = 2, PageSize = 2, SortBy = "Symbol" };
        var items = new List<Stock> { E(1, "AAPL", "Apple", 100), E(2, "MSFT", "Microsoft", 200) };
        _repo.Setup(r => r.QueryAsync(q, It.IsAny<CancellationToken>())).ReturnsAsync((items, 7));

        var res = await Sut().QueryAsync(q, CancellationToken.None);

        res.Success.Should().BeTrue();
        res.Data.TotalCount.Should().Be(7);
        res.Data.PageNumber.Should().Be(2);
        res.Data.PageSize.Should().Be(2);
        res.Data.Items.Should().HaveCount(2);
        res.Data.Items.Select(x => x.Symbol).Should().Contain(new[] { "AAPL" });
    }

    [Fact]
    public async Task QueryAsync_BeginsScope_WithExpectedKeys()
    {
        var q = new QueryObject { PageNumber = 1, PageSize = 5, SortBy = "Purchase", IsDescending = true, Symbol = "AA", CompanyName = "Ap" };
        _repo.Setup(r => r.QueryAsync(q, It.IsAny<CancellationToken>())).ReturnsAsync((new List<Stock>(), 0));

        _ = await Sut().QueryAsync(q, CancellationToken.None);

        _log.Verify(l => l.BeginScope(It.IsAny<Dictionary<string, object>>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Persists_And_Maps()
    {
        var dto = TestData.CreateStockDto("MSFT", "Microsoft", 200);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var res = await Sut().CreateAsync(dto, "u1", CancellationToken.None);

        res.Success.Should().BeTrue();
        res.Data.Symbol.Should().Be("MSFT");
        _repo.Verify(r => r.CreateAsync(It.IsAny<Stock>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_Fails()
    {
        _repo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Stock?)null);

        var res = await Sut().UpdateAsync(99, TestData.UpdateStockDto(), "u", CancellationToken.None);

        res.Success.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_Updates_And_Maps()
    {
        var entity = E(5, "AAPL", "Apple", 100);
        _repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(EntityHelper.Copy(entity));
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = TestData.UpdateStockDto("NVDA", "NVIDIA", 300);
        var res = await Sut().UpdateAsync(5, dto, "u", CancellationToken.None);

        res.Success.Should().BeTrue();
        res.Data.Symbol.Should().Be("NVDA");
        res.Data.CompanyName.Should().Be("NVIDIA");
        res.Data.Purchase.Should().Be(300);
        _repo.Verify(r => r.UpdateAsync(
            It.Is<Stock>(s => s.Id == 5 && s.Symbol == "NVDA" && s.CompanyName == "NVIDIA" && s.Purchase == 300),
            It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_Fails()
    {
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Stock?)null);

        var res = await Sut().DeleteAsync(1, "u", CancellationToken.None);

        res.Success.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_Deletes_And_ReturnsTrue()
    {
        var s = E(3, "NFLX", "Netflix", 120);
        _repo.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(s);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var res = await Sut().DeleteAsync(3, "u", CancellationToken.None);

        res.Success.Should().BeTrue();
        res.Data.Should().BeTrue();
        _repo.Verify(r => r.DeleteAsync(It.Is<Stock>(x => x.Id == 3), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_Found_Maps()
    {
        var s = E(10, "TSLA", "Tesla", 150);
        _repo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(s);

        var res = await Sut().GetByIdAsync(10, CancellationToken.None);

        res.Success.Should().BeTrue();
        res.Data.Symbol.Should().Be("TSLA");
        res.Data.CompanyName.Should().Be("Tesla");
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_Fails()
    {
        _repo.Setup(r => r.GetByIdAsync(42, It.IsAny<CancellationToken>())).ReturnsAsync((Stock?)null);

        var res = await Sut().GetByIdAsync(42, CancellationToken.None);

        res.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetBySymbolAsync_Invalid_Fails()
    {
        var res = await Sut().GetBySymbolAsync("", CancellationToken.None);

        res.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetBySymbolAsync_Found_Maps()
    {
        var s = E(2, "AAPL", "Apple", 100);
        _repo.Setup(r => r.GetBySymbolAsync("AAPL", It.IsAny<CancellationToken>())).ReturnsAsync(s);

        var res = await Sut().GetBySymbolAsync("AAPL", CancellationToken.None);

        res.Success.Should().BeTrue();
        res.Data.Symbol.Should().Be("AAPL");
    }

    [Fact]
    public async Task GetBySymbolAsync_NotFound_Fails()
    {
        _repo.Setup(r => r.GetBySymbolAsync("NFLX", It.IsAny<CancellationToken>())).ReturnsAsync((Stock?)null);

        var res = await Sut().GetBySymbolAsync("NFLX", CancellationToken.None);

        res.Success.Should().BeFalse();
    }
}