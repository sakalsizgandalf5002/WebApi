using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Serilog.Context;
using System.Text.Json;
using System.Security;

namespace Api.Middleware
{
    public sealed class ExceptionMiddleware
    {
        private const string ProblemJson = "application/problem+json";
        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            var reqId = (ctx.Request.Headers.TryGetValue("X-Request-ID", out var inHdr) && !string.IsNullOrWhiteSpace(inHdr))
                ? inHdr.ToString()
                : ctx.TraceIdentifier;
            ctx.Response.Headers["X-Request-ID"] = reqId;

            using (LogContext.PushProperty("RequestId", reqId))
            {
                try
                {
                    await _next(ctx);
                }
                catch (ValidationException vex)
                {
                    _logger.LogWarning(vex, "Validation failed");
                    if (!ctx.Response.HasStarted)
                    {
                        var errors = vex.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());

                        var problem = new ValidationProblemDetails(errors)
                        {
                            Title = "Validation Failed",
                            Status = StatusCodes.Status400BadRequest,
                            Instance = ctx.Request.Path
                        };
                        problem.Type = "validation.failed";
                        problem.Extensions["correlationId"] = reqId;
                        problem.Extensions["traceId"] = ctx.TraceIdentifier;
                        problem.Extensions["timestamp"] = DateTime.UtcNow;

                        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
                        ctx.Response.ContentType = ProblemJson;
                        await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOpts));
                    }
                }
                catch (UnauthorizedAccessException uex)
                {
                    _logger.LogWarning(uex, "Unauthorized");
                    if (!ctx.Response.HasStarted)
                    {
                        var problem = NewProblem(
                            title: "Unauthorized",
                            status: StatusCodes.Status401Unauthorized,
                            ctx: ctx,
                            detail: "Authentication is required.",
                            reqId: reqId,
                            type: "auth.unauthorized"
                        );

                        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        ctx.Response.ContentType = ProblemJson;
                        ctx.Response.Headers["WWW-Authenticate"] = "Bearer";
                        await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOpts));
                    }
                }
                catch (SecurityException secx)
                {
                    _logger.LogWarning(secx, "Forbidden");
                    if (!ctx.Response.HasStarted)
                    {
                        var problem = NewProblem(
                            title: "Forbidden",
                            status: StatusCodes.Status403Forbidden,
                            ctx: ctx,
                            detail: "You do not have permission to access this resource.",
                            reqId: reqId,
                            type: "auth.forbidden"
                        );

                        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                        ctx.Response.ContentType = ProblemJson;
                        await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOpts));
                    }
                }
                catch (DbUpdateConcurrencyException cex)
                {
                    _logger.LogWarning(cex, "Concurrency conflict");
                    if (!ctx.Response.HasStarted)
                    {
                        var problem = NewProblem(
                            title: "Conflict",
                            status: StatusCodes.Status409Conflict,
                            ctx: ctx,
                            detail: "The resource was modified by another process. Please reload and try again.",
                            reqId: reqId,
                            type: "db.concurrency_conflict"
                        );

                        ctx.Response.StatusCode = StatusCodes.Status409Conflict;
                        ctx.Response.ContentType = ProblemJson;
                        await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOpts));
                    }
                }
                catch (DbUpdateException duex) 
                {
                    _logger.LogError(duex, "Database update error");
                    if (!ctx.Response.HasStarted)
                    {
                         var problem = NewProblem(
                            title: "Conflict",
                            status: StatusCodes.Status409Conflict,
                            ctx: ctx,
                            detail: "A data persistence error occurred",
                            reqId: reqId,
                            type: "db.update_failure");
                        ctx.Response.StatusCode = StatusCodes.Status409Conflict;
                        ctx.Response.ContentType = ProblemJson;
                        await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOpts));                       
                    }
                }
                catch (NotImplementedException niex)
                {
                    _logger.LogWarning(niex, "Not implemented");
                    if (!ctx.Response.HasStarted)
                    {
                        var problem = NewProblem(
                            title: "Not Implemented",
                            status: StatusCodes.Status501NotImplemented,
                            ctx: ctx,
                            detail: SafeDetail(_env, niex, "This functionality is not implemented."),
                            reqId: reqId,
                            type: "api.not_implemented"
                        );

                        ctx.Response.StatusCode = StatusCodes.Status501NotImplemented;
                        ctx.Response.ContentType = ProblemJson;
                        await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOpts));
                    }
                }
                catch (KeyNotFoundException kex)
                {
                    _logger.LogInformation(kex, "Not found");
                    if (!ctx.Response.HasStarted)
                    {
                        var detail = string.IsNullOrWhiteSpace(kex.Message)
                            ? "The requested resource was not found."
                            : SafeDetail(_env, kex, "The requested resource was not found.");

                        var problem = NewProblem(
                            title: "Not Found",
                            status: StatusCodes.Status404NotFound,
                            ctx: ctx,
                            detail: detail,
                            reqId: reqId,
                            type: "resource.not_found"
                        );

                        ctx.Response.StatusCode = StatusCodes.Status404NotFound;
                        ctx.Response.ContentType = ProblemJson;
                        await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOpts));
                    }
                }
                catch (BadHttpRequestException brex)
                {
                    _logger.LogWarning(brex, "Bad request");
                    if (!ctx.Response.HasStarted)
                    {
                        var detail = SafeDetail(_env, brex, "Malformed request.");
                        var problem = NewProblem(
                            title: "Bad Request",
                            status: StatusCodes.Status400BadRequest,
                            ctx: ctx,
                            detail: detail,
                            reqId: reqId,
                            type: "http.bad_request"
                        );

                        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
                        ctx.Response.ContentType = ProblemJson;
                        await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOpts));
                    }
                }
                catch (OperationCanceledException ocex) when (ctx.RequestAborted.IsCancellationRequested)
                {
                    _logger.LogInformation(ocex, "Request was canceled by the client");
                    if (!ctx.Response.HasStarted)
                    {
                        var problem = NewProblem(
                            title: "Client Closed Request",
                            status: 499, // nginx-style client closed
                            ctx: ctx,
                            detail: "The client canceled the request.",
                            reqId: reqId,
                            type: "client.canceled"
                        );

                        ctx.Response.StatusCode = 499;
                        ctx.Response.ContentType = ProblemJson;
                        await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOpts));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception");
                    if (!ctx.Response.HasStarted)
                    {
                        var problem = NewProblem(
                            title: "Server Error",
                            status: StatusCodes.Status500InternalServerError,
                            ctx: ctx,
                            detail: SafeDetail(_env, ex, "An unexpected error occurred."),
                            reqId: reqId,
                            type: "error.unhandled"
                        );

                        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        ctx.Response.ContentType = ProblemJson;
                        await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOpts));
                    }
                }
            }
        }

        private static string SafeDetail(IHostEnvironment env, Exception ex, string fallback)
            => env.IsDevelopment() ? (ex.Message ?? fallback) : fallback;

        private static ProblemDetails NewProblem(string title, int status, HttpContext ctx, string detail, string reqId, string type)
        {
            var pd = new ProblemDetails
            {
                Title = title,
                Status = status,
                Detail = detail,
                Instance = ctx.Request.Path,
                Type = type
            };
            pd.Extensions["traceId"] = ctx.TraceIdentifier;
            pd.Extensions["correlationId"] = reqId;
            pd.Extensions["timestamp"] = DateTime.UtcNow;
            return pd;
        }
    }

    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiExceptionMiddleware(this IApplicationBuilder app)
            => app.UseMiddleware<ExceptionMiddleware>();
    }
}
