using System;
using System.Collections.Generic;
using ValidationException = FluentValidation.ValidationException;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Middleware
{
    public sealed class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext ctx)
        {
            try
            {
                await _next(ctx);
            }
            catch (ValidationException vex)
            {
                _logger.LogWarning(vex, "Validation failed");
                ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsJsonAsync(new
                {
                    title = "Validation Failed",
                    status = 400,
                    errors = vex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }),
                    traceId = ctx.TraceIdentifier

                });
            }
            catch (UnauthorizedAccessException uex)
            {
                _logger.LogWarning(uex, "Unauthorized");
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                ctx.Response.ContentType = "Application/json";
                await ctx.Response.WriteAsJsonAsync(new
                {
                    title = "Unauthorized",
                    status = 401,
                    traceId = ctx.TraceIdentifier
                });
            }
            catch (KeyNotFoundException kex)
            {
                _logger.LogInformation(kex, "Not found");
                ctx.Response.StatusCode = StatusCodes.Status404NotFound;
                ctx.Response.ContentType = "Application/json";
                await ctx.Response.WriteAsJsonAsync(new
                {
                    title = "Not found",
                    status = 404,
                    traceId = ctx.TraceIdentifier
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsJsonAsync(new
                {
                    title = "Server Error",
                    status = 500,
                    traceId = ctx.TraceIdentifier
                });
            }
        }
    }
}