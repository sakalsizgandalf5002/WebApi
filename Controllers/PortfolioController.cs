using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Api.Extensions;
using Api.Interfaces;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Api.Controllers
{
    [Route("api/portfolio")]
    [ApiController]
    public class PortfolioController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IStockRepo _stockRepo;
        private readonly IPortfolioRepo _portfolioRepo;
        public PortfolioController(UserManager<AppUser> userManager, IStockRepo stockRepo, IPortfolioRepo portfolioRepo)
        {
            _userManager = userManager;
            _stockRepo = stockRepo;
            _portfolioRepo = portfolioRepo;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserPortfolio()
        {
            var userId = User.GetUserId();
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var appUser = await _userManager.FindByIdAsync(userId);
            if (appUser is null) return Unauthorized();

            var userPortfolio = await _portfolioRepo.GetUserPortfolio(appUser);
            return Ok(userPortfolio);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddPortfolio([FromQuery] string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol)) return BadRequest("Symbol is required.");

            var userId = User.GetUserId();
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var appUser = await _userManager.FindByIdAsync(userId);
            if (appUser is null) return Unauthorized();

            var stock = await _stockRepo.GetBySymbolAsync(symbol);
            if (stock == null) return NotFound("Stock not found");

            var userPortfolio = await _portfolioRepo.GetUserPortfolio(appUser);
            if (userPortfolio.Any(s => s.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase)))
                return BadRequest("Stock already in portfolio");

            var portfolioModel = new Portfolio
            {
                StockId = stock.Id,
                AppUserId = appUser.Id
            };

            await _portfolioRepo.CreateAsync(portfolioModel);

            if (portfolioModel == null)
                return StatusCode(500, "An error occurred while adding the stock to the portfolio");

            return Created();
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeletePortfolio([FromQuery] string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol)) return BadRequest("Symbol is required.");

            var userId = User.GetUserId();
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var appUser = await _userManager.FindByIdAsync(userId);
            if (appUser is null) return Unauthorized();

            var userPortfolio = await _portfolioRepo.GetUserPortfolio(appUser);

            var filteredStock = userPortfolio.Where(s => s.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase)).ToList();
            if (filteredStock.Count == 1)
            {
                await _portfolioRepo.DeletePortfolio(appUser, symbol);
            }
            else
            {
                return BadRequest("Stock is not in your portfolio");
            }
            return Ok();
        }
    }
}
