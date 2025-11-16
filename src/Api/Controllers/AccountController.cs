using Api.DTOs.Account;
using Api.Interfaces;
using Api.Interfaces.IService;
using Api.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : AppControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IMapper _mapper;
        private readonly IRefreshTokenService _rts;

        public AccountController(UserManager<AppUser> userManeger, ITokenService tokenService,
            SignInManager<AppUser> signInManager, IMapper mapper, IRefreshTokenService rts)
        {
            _userManager = userManeger;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _mapper = mapper;
            _rts = rts;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            
            var user = await _userManager.Users
                .FirstOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());
            
            if (user == null)
                return Unauthorized("Invalid username or password");
            
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
                return Unauthorized("Invalid username or password");
            
            var dto = _mapper.Map<NewUserDto>(user);
            dto.Token = _tokenService.CreateToken(user);
            return Ok(dto);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var appUser = new AppUser
            {
                UserName = registerDto.Username,
                Email = registerDto.Email,
            };
            
            var createdUser = await _userManager.CreateAsync(appUser, registerDto.Password);

            if (!createdUser.Succeeded)
            {
                var errors = createdUser.Errors.Select(x => x.Description);
                return BadRequest(new { errors });
            }
            
            var roleResult = await _userManager.AddToRoleAsync(appUser, "User");
            
            if (!roleResult.Succeeded)
            {
                var errors = roleResult.Errors.Select(x => x.Description);
                return StatusCode(500, new { errors });
            }
            
            var dto = _mapper.Map<NewUserDto>(appUser);
            dto.Token = _tokenService.CreateToken(appUser);
            return Ok(dto);
        }

        [HttpPost("Refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            
            var ip = GetIpAddress();
            var (access, refresh) = await _rts.RotateAsync(dto.RefreshToken, ip);

            var response = new AuthResponseDto
            {
                AccessToken = access,
                RefreshToken = refresh.Token,
                RefreshTokenExpires = refresh.Expires
            };

            return Ok(response);
        }

        [HttpPost("Revoke")]
        [Authorize]
        public async Task<IActionResult> Revoke([FromBody] RefreshRequestDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            
            var ip = GetIpAddress();

            await _rts.RevokeAsync(dto.RefreshToken, ip, "User revoked");

            return NoContent();
        }
    }
}
    

