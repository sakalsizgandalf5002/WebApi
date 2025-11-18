using Api.DTOs;
using Api.DTOs.Stock;
using Api.Extensions;
using Api.Helpers;
using Api.Interfaces.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/stock")]
    [ApiController]
    public class StockController : AppControllerBase
    {
        private readonly IStockService _stockService;
        public StockController(IStockService stockService) => _stockService = stockService;

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] QueryObject query, CancellationToken ct)
        {
            var res = await _stockService.QueryAsync(query, ct);
            return res.ToActionResult();
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
        {
            var res = await _stockService.GetByIdAsync(id, ct);
            return res.ToActionResult();

        }

        [HttpGet("symbol/{symbol}")]
        public async Task<IActionResult> GetBySymbol([FromRoute] string symbol, CancellationToken ct)
        {
            var res = await _stockService.GetBySymbolAsync(symbol, ct);
            return res.ToActionResult();

        }

        [HttpPost]
        [Authorize]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] CreateStockRequestDto dto, CancellationToken ct)
        {
           var res =  await _stockService.CreateAsync(dto, UserId, ct);
           return res.ToActionResult();
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateStockRequestDto dto, CancellationToken ct)
        {
           var res = await _stockService.UpdateAsync(id, dto, UserId, ct);
           return res.ToActionResult();
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
           
                var res = await _stockService.DeleteAsync(id, UserId, ct);
                return res.ToActionResult();
            
        }
    }
}
