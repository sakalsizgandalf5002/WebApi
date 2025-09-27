using Microsoft.AspNetCore.Mvc; 
using Api.Models;
using Api.DTOs.Comment;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Api.Interfaces.IService;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers
{
    [Route("api/comment")]
    [ApiController]
    public class CommentController : AppControllerBase
    {
        private readonly ICommentService _service;
        private readonly UserManager<AppUser> _userManager;

        public CommentController(ICommentService service, UserManager<AppUser> userManager)
        {
            _service = service;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var res = await _service.GetAllAsync(ct);
            if (!res.Success) return BadRequest(res.Message);
            return Ok(res.Data);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
        {
            var res = await _service.GetByIdAsync(id, ct);
            if (!res.Success) return NotFound(res.Message);
            return Ok(res.Data);
        }

        [Authorize]
        [HttpPost("{stockId:int}")]
        public async Task<IActionResult> Create([FromRoute] int stockId, [FromBody] CreateCommentDto dto, CancellationToken ct)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            var res = await _service.CreateAsync(dto, user.Id, stockId, ct);
            if (!res.Success) return BadRequest(res.Message);

            return CreatedAtAction(nameof(GetById), new { id = res.Data.Id }, res.Data);
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCommentRequestDto dto, CancellationToken ct)
        {
           var user = await GetCurrentUserAsync();
           if (user == null) return Unauthorized();

           var res = await _service.UpdateAsync(id, dto, user.Id, ct);
           if (!res.Success && res.Message == "not_found") return NotFound();
           if (!res.Success && res.Message == "forbidden") return Forbid();

           return Ok(res.Data);
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            var res = await _service.DeleteAsync(id, user.Id, ct);
            if (!res.Success && res.Message == "not_found") return NotFound();
            if (!res.Success && res.Message == "forbidden") return Forbid();

            return NoContent();
        }
        
        private async Task<AppUser?> GetCurrentUserAsync()
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User?.FindFirst("sub")?.Value;

            if (!string.IsNullOrWhiteSpace(userId))
                return new AppUser { Id = userId };

            var name = User?.Identity?.Name;
            var email = User?.FindFirst("email")?.Value
                        ?? User?.FindFirst(ClaimTypes.Email)?.Value;
            var given = User?.FindFirst("given_name")?.Value;

            var key = name ?? email ?? given;
            if (string.IsNullOrWhiteSpace(key)) return null;

            return await _userManager.Users
                .FirstOrDefaultAsync(u => u.UserName == key || u.Email == key);
        }
    }
}
