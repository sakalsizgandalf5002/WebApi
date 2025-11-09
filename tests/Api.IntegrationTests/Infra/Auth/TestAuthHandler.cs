using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infra.Auth;

public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string Scheme = "Test";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) : base(options, logger, encoder, clock) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? rolesHeader = null;
        if (Request.Headers.TryGetValue("X-Test-Roles", out var rolesValues))
            rolesHeader = rolesValues.ToString();

        var roles = string.IsNullOrWhiteSpace(rolesHeader)
            ? new[] { "User" }
            : rolesHeader.Split(',',
                System.StringSplitOptions.TrimEntries | System.StringSplitOptions.RemoveEmptyEntries);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user"),
            new Claim(ClaimTypes.Name, "tester")
        };
        foreach (var r in roles)
            claims.Add(new Claim(ClaimTypes.Role, r));

        if (Request.Headers.TryGetValue("X-Test-Claims", out var permValues))
        {
            var perms = permValues.ToString().Split(',',
                System.StringSplitOptions.TrimEntries | System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in perms)
                claims.Add(new Claim("permission", p));
        }

        var identity = new ClaimsIdentity(claims, Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}