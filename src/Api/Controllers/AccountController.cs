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
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());
            if (user == null) return Unauthorized("Invalid username!");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded) return Unauthorized("Username not found or invalid password!");

            var dto = _mapper.Map<NewUserDto>(user);
            dto.Token = _tokenService.CreateToken(user);
            return Ok(dto);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var appUser = new AppUser
            {
                UserName = registerDto.Username,
                Email = registerDto.Email
            };

            var createdUser = await _userManager.CreateAsync(appUser, registerDto.Password);
            if (createdUser.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(appUser, "User");
                if (roleResult.Succeeded)
                {
                    var dto = _mapper.Map<NewUserDto>(appUser);
                    dto.Token = _tokenService.CreateToken(appUser);
                    return Ok(dto);
                }

                return BadRequest(roleResult.Errors);
            }

            return StatusCode(500, createdUser.Errors);
        }

        [HttpPost("Refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto dto)
        {
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
            var ip = GetIpAddress();

            await _rts.RevokeAsync(dto.RefreshToken, ip, "User revoked");

            return NoContent();
        }
    }
}
    

