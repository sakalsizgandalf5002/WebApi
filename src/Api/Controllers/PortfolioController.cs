using Api.Extensions;
using Api.Interfaces.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/portfolio")]
    public class PortfolioController : AppControllerBase
    {
        private readonly IPortfolioService _service;

        public PortfolioController(IPortfolioService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserPortfolio(CancellationToken ct)
        {
            var res = await _service.GetUserPortfolioAsync(UserId, ct);
            return res.ToActionResult();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddPortfolio([FromQuery] string symbol, CancellationToken ct)
        {
            var res = await _service.AddAsync(UserId, symbol, ct);
            return res.ToActionResult();
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeletePortfolio([FromQuery] string symbol, CancellationToken ct)
        {
            var res = await _service.RemoveAsync(UserId, symbol, ct);
            return res.ToActionResult();
        }
    }
}