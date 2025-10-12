using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Api.Extensions
{
    public static class ClaimExtensions
    {
        public static string? GetUserId(this ClaimsPrincipal p) =>
            p?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? p?.FindFirst("sub")?.Value;

        public static string? GetUserName(this ClaimsPrincipal p) =>
            p?.Identity?.Name
            ?? p?.FindFirstValue(JwtRegisteredClaimNames.Email)
            ?? p?.FindFirstValue(ClaimTypes.Name);
    }
}