using Serilog;
using Serilog.Events;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Api.Data;
using Api.Models;
using Api.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;

// Serilog bootstrap
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.Development.json", optional: true)
        .AddEnvironmentVariables()
        .Build())
    .Enrich.FromLogContext()
    .MinimumLevel.Override("Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware", LogEventLevel.Error)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

// Portlarý sabitle
builder.WebHost.ConfigureKestrel(k =>
{
    k.ListenLocalhost(5254);                 // HTTP
    k.ListenLocalhost(7254, o => o.UseHttps()); // HTTPS
});

// Services
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
     .EnableDetailedErrors(builder.Environment.IsDevelopment())
     .EnableSensitiveDataLogging(builder.Environment.IsDevelopment()));

builder.Services.AddIdentity<AppUser, IdentityRole>(opt =>
{
    opt.Password.RequireDigit = true;
    opt.Password.RequireLowercase = true;
    opt.Password.RequireUppercase = true;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequiredLength = 12;
}).AddEntityFrameworkStores<AppDbContext>();

builder.Services.Configure<PepperedPasswordHasher.Options>(builder.Configuration.GetSection("Security"));
builder.Services.AddScoped<IPasswordHasher<AppUser>, PepperedPasswordHasher>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"]))
        };
    });

builder.Services.AddControllers().AddNewtonsoftJson(x =>
    x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id="Bearer" } }, Array.Empty<string>() }
    });
});

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
    .AddDbContextCheck<AppDbContext>("sql", tags: new[] { "ready" });

var app = builder.Build();

// RequestId + UserId zenginleþtirme
app.Use(async (ctx, next) =>
{
    var reqId = ctx.Request.Headers.TryGetValue("X-Request-ID", out var h) && !string.IsNullOrWhiteSpace(h)
        ? h.ToString() : ctx.TraceIdentifier;
    ctx.Response.Headers["X-Request-ID"] = reqId;

    using (Serilog.Context.LogContext.PushProperty("RequestId", reqId))
    using (Serilog.Context.LogContext.PushProperty("UserId", ctx.User?.Identity?.IsAuthenticated == true ? (ctx.User.Identity?.Name ?? "anon") : "anon"))
    {
        await next();
    }
});

app.UseSerilogRequestLogging(o =>
{
    o.MessageTemplate = "HTTP {RequestMethod} {RequestPath} by {UserId} [{RequestId}] responded {StatusCode} in {Elapsed:0.0000} ms from {ClientIp} {QueryString}";
    o.GetLevel = (http, elapsed, ex) =>
    {
        if (ex != null || http.Response.StatusCode >= 500) return LogEventLevel.Error;
        if (http.Response.StatusCode >= 400) return LogEventLevel.Warning;
        return app.Environment.IsDevelopment() ? LogEventLevel.Debug : LogEventLevel.Information;
    };
    o.EnrichDiagnosticContext = (dc, http) =>
    {
        dc.Set("ClientIp", http.Connection.RemoteIpAddress?.ToString() ?? "unknown");
        dc.Set("QueryString", http.Request.QueryString.HasValue ? http.Request.QueryString.Value : "");
    };
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseRewriter(new RewriteOptions().AddRedirect("^$", "swagger", 302));
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

static Task HealthWrite(HttpContext ctx, HealthReport rep)
{
    ctx.Response.ContentType = "application/json; charset=utf-8";
    var payload = new
    {
        status = rep.Status.ToString(),
        duration = $"{rep.TotalDuration.TotalMilliseconds:F0} ms",
        results = rep.Entries.Select(e => new
        {
            key = e.Key,
            status = e.Value.Status.ToString(),
            error = e.Value.Exception?.Message ?? "none"
        })
    };
    var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
    return ctx.Response.WriteAsync(json);
}

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = r => r.Tags.Contains("live"), ResponseWriter = HealthWrite });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = r => r.Tags.Contains("ready"), ResponseWriter = HealthWrite });

try
{
    Log.Information("Starting WebApi on http://localhost:5254 and https://localhost:7254");
    app.Run(); // bloklar; kapanma çaðrýsý YOK
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
