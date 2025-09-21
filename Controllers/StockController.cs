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
        public StockController(IStockService stockService)
        {
            _stockService = stockService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] QueryObject query)
        {
            var result = await _stockService.QueryAsync(query);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Data);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var result = await _stockService.GetByIdAsync(id);
            if (!result.Success) return NotFound(result.Message);
            return Ok(result.Data);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateStockRequestDto stockDto)
        {
            if (string.IsNullOrWhiteSpace(UserId)) return Unauthorized();

            var result = await _stockService.CreateAsync(stockDto, UserId);
            if (!result.Success) return BadRequest(result.Message);
            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateStockRequestDto updateDto)
        {
            if (string.IsNullOrWhiteSpace(UserId)) return Unauthorized();

            var result = await _stockService.UpdateAsync(id, updateDto, UserId);
            if (!result.Success) return NotFound(result.Message);
            return Ok(result.Data);
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            if (string.IsNullOrWhiteSpace(UserId)) return Unauthorized();

            var result = await _stockService.DeleteAsync(id, UserId);
            if (!result.Success) return NotFound(result.Message);
            return NoContent();
        }
    }
}
