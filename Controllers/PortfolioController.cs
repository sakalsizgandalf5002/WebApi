using System.Threading;
using System.Threading.Tasks;
using Api.Interfaces.IService;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/portfolio")]
    [ApiController]
    public class PortfolioController : AppControllerBase
    {
        private readonly IPortfolioService _service;
        private readonly UserManager<AppUser> _userManager;

        public PortfolioController(IPortfolioService service, UserManager<AppUser> userManager)
        {
            _service = service;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserPortfolio(CancellationToken ct)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Unauthorized();

            var res = await _service.GetUserPortfolioAsync(user, ct);
            if (!res.Success) return BadRequest(res.Message);
            return Ok(res.Data);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddPortfolio([FromQuery] string symbol, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(symbol)) return BadRequest("Symbol is required.");

            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Unauthorized();

            var res = await _service.AddAsync(user, symbol, ct);
            if (!res.Success) return NotFound(res.Message); 
            return Ok();
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeletePortfolio([FromQuery] string symbol, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(symbol)) return BadRequest("Symbol is required.");

            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Unauthorized();

            var res = await _service.RemoveAsync(user, symbol, ct);
            if (!res.Success) return NotFound(res.Message); // "not_found"
            return NoContent();
        }
    }
}
