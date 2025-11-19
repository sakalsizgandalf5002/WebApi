using Serilog;
using Serilog.Events;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Api.Data;
using Api.Models;
using Api.Service;
using Api.Repo;
using Api.Security;
using Api.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using Api.Interfaces.IRepo;
using Api.Interfaces.IService;
using Api.Middleware;
using Api.Options;
using Api.Validators.Comment;
using FluentValidation;
using FluentValidation.AspNetCore;

var tempConfig = new ConfigurationBuilder()
    .AddUserSecrets<Program>(optional: true)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile("appsettings.Development.json", optional: true)
        .AddUserSecrets<Program>(optional: true)
        .AddEnvironmentVariables()
        .Build())
    .Enrich.FromLogContext()
    .MinimumLevel.Override("Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware", LogEventLevel.Error)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets<Program>(optional: true);

builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));


builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection("JWT"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var allowed = (builder.Configuration["ALLOWED_ORIGINS"] ?? "")
    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("Default", p =>
    {
        if (allowed.Length > 0)
            p.WithOrigins(allowed).AllowAnyHeader().AllowAnyMethod();
        else
            p.WithOrigins("http://localhost:5173", "http://localhost:3000")
                .AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddRateLimiter(o =>
{
    o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
    {
        var path = ctx.Request.Path.Value ?? string.Empty;

        if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/health", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase))
        {
            return RateLimitPartition.GetNoLimiter("infra");
        }

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "anon",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,               
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });
});

builder.Services.AddDbContext<AppDbContext>(o =>
{
    var cs = builder.Configuration.GetConnectionString("DefaultConnection")
             ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

    if (string.IsNullOrWhiteSpace(cs))
        throw new InvalidOperationException("No connection string found. Check env var ConnectionStrings__DefaultConnection.");

    o.UseSqlServer(cs)
        .EnableDetailedErrors(builder.Environment.IsDevelopment())
        .EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
});

builder.Services
    .AddIdentityCore<AppUser>(opt =>
    {
        opt.Password.RequireDigit = true;
        opt.Password.RequireLowercase = true;
        opt.Password.RequireUppercase = true;
        opt.Password.RequireNonAlphanumeric = false;
        opt.Password.RequiredLength = 12;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme             = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(o =>
    {
        var key = builder.Configuration["JWT:SigningKey"];
        if (string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException("JWT:SigningKey not configured.");

        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("RequireAdmin", p => p.RequireRole("Admin"));
});

builder.Services.AddAutoMapper(
    cfg => { },
    typeof(Program).Assembly,
    typeof(AccountMapper).Assembly,
    typeof(StockMapper).Assembly,
    typeof(CommentMapper).Assembly
);

builder.Services.AddScoped<IStockRepo, StockRepo>();
builder.Services.AddScoped<ICommentRepo, CommentRepo>();
builder.Services.AddScoped<IPortfolioRepo, PortfolioRepo>();
builder.Services.AddScoped<IRefreshTokenRepo, RefreshTokenRepo>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<ICommentService,  CommentService>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

builder.Services.Configure<PepperedPasswordHasher.Options>(builder.Configuration.GetSection("Security"));
builder.Services.AddScoped<IPasswordHasher<AppUser>, PepperedPasswordHasher>();

builder.Services.AddControllers().AddNewtonsoftJson(x =>
        x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore)
    .AddFluentValidation(fv => { });
builder.Services.AddValidatorsFromAssemblyContaining<CreateCommentDtoValidator>();
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

app.UseApiExceptionMiddleware();

if (!app.Environment.IsDevelopment())
    app.UseHsts();
app.UseHttpsRedirection();

app.Use ((ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["Referrer-Policy"] = "no-referrer";
    ctx.Response.Headers["Permission-Policy"] = "geolocation=()";
    return next();
});

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
    o.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms from {ClientIp} {UserAgent} {QueryString}";

    o.GetLevel = (http, elapsed, ex) =>
    {
        if (ex != null || http.Response.StatusCode >= 500)
            return LogEventLevel.Error;

        if (http.Response.StatusCode == StatusCodes.Status404NotFound)
            return LogEventLevel.Information;

        if (http.Response.StatusCode >= 400)
            return LogEventLevel.Warning;

        return app.Environment.IsDevelopment()
            ? LogEventLevel.Debug
            : LogEventLevel.Information;
    };

    o.EnrichDiagnosticContext = (dc, http) =>
    {
        dc.Set("ClientIp", http.Connection.RemoteIpAddress?.ToString() ?? "unknown");
        dc.Set("QueryString", http.Request.QueryString.HasValue ? http.Request.QueryString.Value : "");
        dc.Set("UserAgent", http.Request.Headers["User-Agent"].FirstOrDefault() ?? "unknown");

        if (http.Request.Headers.ContainsKey("Authorization"))
            dc.Set("Authorization", "redacted");
    };
});

app.UseCors("Default");
app.UseRateLimiter();


app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseRewriter(new RewriteOptions().AddRedirect("^$", "swagger", 302));
}

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
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
public partial class Program { }
