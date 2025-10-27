using System.Threading;
using System.Threading.Tasks;
using Api.DTOs;
using Api.DTOs.Stock;
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
            var result = await _stockService.QueryAsync(query, ct);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Data);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
        {
            var result = await _stockService.GetByIdAsync(id, ct);
            if (!result.Success) return NotFound(result.Message);
            return Ok(result.Data);
        }

        [HttpGet("symbol/{symbol}")]
        public async Task<IActionResult> GetBySymbol([FromRoute] string symbol, CancellationToken ct)
        {
            var result = await _stockService.GetBySymbolAsync(symbol, ct);
            if (!result.Success) return NotFound(result.Message);
            return Ok(result.Data);
        }

        [HttpPost]
        [Authorize]
        [Consumes("application/json")]
        public async Task<IActionResult> Create([FromBody] CreateStockRequestDto dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(UserId)) return Unauthorized();
            var result = await _stockService.CreateAsync(dto, UserId, ct);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Data);
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateStockRequestDto dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(UserId)) return Unauthorized();
            var result = await _stockService.UpdateAsync(id, dto, UserId, ct);
            if (!result.Success) return NotFound(result.Message);
            return Ok(result.Data);
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(UserId)) return Unauthorized();
            var result = await _stockService.DeleteAsync(id, UserId, ct);
            if (!result.Success) return NotFound(result.Message);
            return NoContent();
        }
    }
}
