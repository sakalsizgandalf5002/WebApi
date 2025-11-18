using Microsoft.AspNetCore.Mvc; 
using Api.Models;
using Api.DTOs.Comment;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Api.Interfaces.IService;
using Api.Extensions;

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
            return res.ToActionResult();
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct)
        {
            var res = await _service.GetByIdAsync(id, ct);
            return res.ToActionResult();
        }

        [Authorize]
        [HttpPost("{stockId:int}")]
        public async Task<IActionResult> Create([FromRoute] int stockId, [FromBody] CreateCommentDto dto, CancellationToken ct)
        {
           var user = await _userManager.GetUserAsync(User);
           
           var res = await _service.CreateAsync(dto, user?.Id,  stockId, ct);
           return res.ToActionResult();
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCommentRequestDto dto, CancellationToken ct)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var res = await _service.UpdateAsync(id, dto, user?.Id,  ct);
            return res.ToActionResult();
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            var user = await _userManager.GetUserAsync(User);
            
            var res = await _service.DeleteAsync(id, user?.Id,  ct);
            return res.ToActionResult();
        }
    }
}
