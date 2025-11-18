using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Api.Interfaces.IService;
using Api.Models;
using Api.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Api.Service
{
    public class TokenService : ITokenService
    {
        private readonly JwtOptions _jwt;
        private readonly SymmetricSecurityKey _key;
        private readonly UserManager<AppUser> _userManager;

        public TokenService(IOptions<JwtOptions> jwtOptions, UserManager<AppUser> userManager)
        {
            _jwt = jwtOptions.Value;
            _userManager = userManager;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey));
        }
        public string CreateToken(AppUser user)
        {
            var claims = new List<Claim>
            {
             new(JwtRegisteredClaimNames.Sub, user.Id),
             new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
             new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
             new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
             new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)

            };
            var roles = _userManager.GetRolesAsync(user).GetAwaiter().GetResult();
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes),
                SigningCredentials = creds,
                Issuer = _jwt.Issuer,
                Audience = _jwt.Audience
            };
            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}