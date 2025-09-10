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
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _service;
        private readonly UserManager<AppUser> _userManager;

        public CommentController(ICommentService service, UserManager<AppUser> userManager)
        {
            _service = service;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var res = await _service.GetAllAsync();
            if (!res.Success) return BadRequest(res.Message);
            return Ok(res.Data);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var res = await _service.GetByIdAsync(id);
            if (!res.Success) return NotFound(res.Message);
            return Ok(res.Data);
        }

        [Authorize]
        [HttpPost("{stockId:int}")]
        public async Task<IActionResult> Create([FromRoute] int stockId, [FromBody] CreateCommentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            var res = await _service.CreateAsync(dto, user.Id, stockId);
            if (!res.Success) return BadRequest(res.Message);

            return CreatedAtAction(nameof(GetById), new { id = res.Data.Id }, res.Data);
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCommentRequestDto dto)
        {
           if (!ModelState.IsValid) return BadRequest(ModelState);

           var user = await GetCurrentUserAsync();
           if (user == null) return Unauthorized();

           var res = await _service.UpdateAsync(id, dto, user.Id);
           if (!res.Success && res.Message == "not_found") return NotFound();
           if (!res.Success && res.Message == "forbidden") return Forbid();

           return Ok(res.Data);
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            var res = await _service.DeleteAsync(id, user.Id);
            if (!res.Success && res.Message == "not_found") return NotFound();
            if (!res.Success && res.Message == "forbidden") return Forbid();

            return NoContent();
        }
        
        private async Task<AppUser?> GetCurrentUserAsync()
        {
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