using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Api.DTOs;
using Api.DTOs.Stock;
using Api.Helpers;
using Api.IntegrationTests.Infra.Db;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Api.IntegrationTests;

[Collection("sqlserver")]
public sealed class StockControllerTests
{
    private readonly SqlServerContainerFixture _fx;

    public StockControllerTests(SqlServerContainerFixture fx) => _fx = fx;

    private HttpClient CreateClient() =>
        new CustomWebApplicationFactory(_fx)
            .CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

    public async Task Initialize()
    {
        await DbReset.ResetAsync(_fx.ConnectionString);
    }
    [Fact]
    public async Task Get_Query_Paginates_Filters_Sorts()
    {
        await Initialize();
        var client = CreateClient();

        var resp = await client.GetAsync("/api/stock?pageNumber=2&pageSize=2&sortBy=Symbol");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var page = await resp.Content.ReadFromJsonAsync<PagedResult<StockDto>>();
        page!.TotalCount.Should().BeGreaterThan(2);
        page.PageNumber.Should().Be(2);
        page.PageSize.Should().Be(2);
        page.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_200_and_400()
    {
        await Initialize();
        var client = CreateClient();
        
        (await client.GetAsync("/api/stock/symbol/AAPL"))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.GetAsync("/api/stock/symbol/NOPE"))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task Create_Update_Delete_Pipeline()
    {
        await Initialize();
        var c = CreateClient();

        var create = new CreateStockRequestDto 
            { Symbol = "NFLX", CompanyName = "Netflix", Purchase = 120 };
        var cr = await c.PostAsJsonAsync("/api/stock", create);
        cr.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await cr.Content.ReadFromJsonAsync<StockDto>();
        created!.Symbol.Should().Be("NFLX");

        var up = new UpdateStockRequestDto
            { Symbol = "NFLX", CompanyName = "Netflix Inc.", Purchase = 130 };
        var ur = await c.PutAsJsonAsync($"/api/stock/{created.Id}", up);
        ur.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await ur.Content.ReadFromJsonAsync<StockDto>();
        updated!.CompanyName.Should().Be("Netflix Inc.");

        var dr = await c.DeleteAsync($"/api/stock/{created.Id}");
        dr.StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await c.GetAsync($"/api/stock/{created.Id}"))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}