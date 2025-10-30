using System.Runtime.InteropServices;
using Moq;
using Api.Service;
using System.Threading;
using System.Threading.Tasks;
using Api.DTOs.Stock;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Api.Interfaces;
using Api.Interfaces.IRepo;
using Api.Models;
using Api.Service;
using Api.Tests.Test.Infrastructure;
using static Api.Tests.Test.Infrastructure.TestData;
using static Api.Tests.Test.Infrastructure.EntityHelper;


namespace Api.Tests.StockTests;

public class StockServiceTests : IClassFixture<AutoMapperFixture>
{
    private readonly IMapper _mapper;
    private readonly Mock<IStockRepo> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ILogger<StockService>> _log = new ();
    
    public StockServiceTests(AutoMapperFixture fx) => _mapper = fx.Mapper;
    
    private StockService SUT()
    => new (_repo.Object, _uow.Object, _mapper, _log.Object);
    
    private static Stock E(int id = 1, string sym = "AAPL", string comp = "Apple", decimal p = 100)
        => Copy(Stock(id, sym, comp, p));

    [Fact]
    public async Task GetById_StockExists_ReturnsDto()
    {
        var entity = E(1, "AAPL");
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var res = await SUT().GetByIdAsync(1, default);

        res.Success.Should().BeTrue();
        res.Data!.Symbol.Should().Be("AAPL");
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsFail()
    {
        _repo.Setup(r => r.GetByIdAsync(420, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stock?)null);
        
        var res = await SUT().GetByIdAsync(420, default);

        res.Success.Should().BeFalse();
        res.Message.Should().Be("not_found");

    }

    [Fact]
    public async Task CreateAsync_ValidInput_PersistsAnd_ReturnsDto()
    {
        var dto = CreateStockDto("AAPL", "Apple", 100);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var res = await SUT().CreateAsync(dto, "user-1", default);

        res.Success.Should().BeTrue();
        _repo.Verify(r => r.CreateAsync(It.IsAny<Stock>(), It.IsAny<CancellationToken>()), Times.Once());
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }


    [Fact]
    public async Task UpdateAsync_EntityExists_UpdatesAnd_ReturnsDto()
    {
        var entity = E(1, "AAPL");
        var dto = UpdateStockDto("APPL", "Apple Inc.", 100);

        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Copy(entity));
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        
        var res = await SUT().UpdateAsync(1, dto, "user-1", default);
        
        res.Success.Should().BeTrue();
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Stock>(), It.IsAny<CancellationToken>()), Times.Once());
    }
}