using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Interfaces;
using Api.Repo;
using Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Api.Service;
using Microsoft.OpenApi.Models;
using Api.Interfaces.IService;
using FluentValidation;
using FluentValidation.AspNetCore;
using Api.Validators.Stock;
using Api.Mappers;
using Serilog;
using Serilog.Events;
using Api.Middleware;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration));


builder.WebHost.ConfigureKestrel(k =>
{
    k.ListenLocalhost(5254);
    k.ListenLocalhost(7254, o => o.UseHttps());
});

Log.Logger = new LoggerConfiguration()
   .ReadFrom.Configuration(builder.Configuration)
   .Enrich.FromLogContext()
   .MinimumLevel.Override("Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware", LogEventLevel.Error)
   .CreateLogger();


builder.Services.AddHttpContextAccessor();


builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
    .AddDbContextCheck<AppDbContext>("sql", tags: new [] { "ready" });

builder.Services.Configure<Api.Security.PepperedPasswordHasher.Options>(
    builder.Configuration.GetSection("Security"));

builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sql =>
        {
            sql.CommandTimeout(30);
            sql.EnableRetryOnFailure(3);
        })
        .EnableDetailedErrors(builder.Environment.IsDevelopment())
        .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
);

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 12;
})
.AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddScoped<IPasswordHasher<AppUser>, Api.Security.PepperedPasswordHasher>();


builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
        options.DefaultChallengeScheme =
        options.DefaultForbidScheme =
        options.DefaultScheme =
        options.DefaultSignInScheme =
        options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
    ValidIssuer = builder.Configuration["JWT:Issuer"],
    ValidateAudience = true,
    ValidAudience = builder.Configuration["JWT:Audience"],
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"])),
    ValidateLifetime = true,
    ClockSkew = TimeSpan.Zero
            
        };
    });

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
builder.Services.AddScoped<IStockRepo, StockRepo>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<ICommentRepo, CommentRepo>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPortfolioRepo, PortfolioRepo>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateStockRequestDtoValidator>();


var app = builder.Build();

app.Use(async (ctx, next) =>
{
    var reqId = ctx.Request.Headers.TryGetValue("X-Request-ID", out var hdr) && !string.IsNullOrWhiteSpace(hdr)
        ? hdr.ToString()
        : ctx.TraceIdentifier;

    ctx.Response.Headers["X-Request-ID"] = reqId;

    using (Serilog.Context.LogContext.PushProperty("RequestId", reqId))
    using (Serilog.Context.LogContext.PushProperty("UserId", ctx.User?.Identity?.IsAuthenticated == true ? ctx.User.Identity?.Name ?? "anon" : "anon"))
    {
        await next();
    }
});

app.UseApiExceptionMiddleware();

app.UseSerilogRequestLogging(opts =>
{
    opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} by {UserId} [{RequestId}] responded {StatusCode} in {Elapsed:0.0000} ms from {ClientIp} {QueryString}";

    opts.GetLevel = (http, elapsed, ex) =>
    {
        if (ex != null || http.Response.StatusCode >= 500) return LogEventLevel.Error;
        if (http.Response.StatusCode >= 400) return LogEventLevel.Warning;
        return app.Environment.IsDevelopment() ? LogEventLevel.Debug : LogEventLevel.Information;
    };

    
    

    opts.EnrichDiagnosticContext = (dc, http) =>
    {
        dc.Set("RequestHost", http.Request.Host.Value);
        dc.Set("UserAgent", http.Request.Headers.UserAgent.ToString());
        dc.Set("ClientIp", http.Connection.RemoteIpAddress?.ToString() ?? "unknown");
        dc.Set("QueryString", http.Request.QueryString.HasValue ? http.Request.QueryString.Value : "");

        var routeValues = http.GetRouteData()?.Values;
        if (routeValues != null)
        {
            if (routeValues.TryGetValue("stockId", out var sid) && sid is not null)
                dc.Set("StockId", sid?.ToString() ?? "");

            if (routeValues.TryGetValue("id", out var cid) && cid is not null)
                dc.Set("CommentId", cid?.ToString() ?? "");
        }

        if (http.Request.Query.TryGetValue("page", out var page) && !string.IsNullOrWhiteSpace(page))
            dc.Set("Page", page.ToString());
        if (http.Request.Query.TryGetValue("size", out var size) && !string.IsNullOrWhiteSpace(size))
            dc.Set("Size", size.ToString());

        var ep = http.GetEndpoint();
        if (ep is not null) dc.Set("EndpointName", ep.DisplayName ?? "");
    };
});
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    var rewrite = new RewriteOptions()
    .AddRedirect("^$", "swagger", 302);

    app.UseRewriter(rewrite);
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

    var json = JsonSerializer.Serialize(
        payload,
        new JsonSerializerOptions { WriteIndented = true }
    );

    var path = ctx.Request.Path.HasValue ? ctx.Request.Path.Value : "";

    if (rep.Status == HealthStatus.Healthy)
        Log.Information("Health {Path} OK in {Ms} ms", path, rep.TotalDuration.TotalMilliseconds);
    else if (rep.Status == HealthStatus.Degraded)
        Log.Warning("Health {Path} DEGRADED in {Ms} ms", path, rep.TotalDuration.TotalMilliseconds);
    else
        Log.Error("Health {Path} UNHEALTHY in {Ms} ms", path, rep.TotalDuration.TotalMilliseconds);

    return ctx.Response.WriteAsync(json);
}


app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live"),
    ResponseWriter = HealthWrite
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready"),
    ResponseWriter = HealthWrite
});



try
{
    Log.Information("Starting WebApi");
    Log.Information("ENV={Env} IsDev={IsDev}", app.Environment.EnvironmentName, app.Environment.IsDevelopment());
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


