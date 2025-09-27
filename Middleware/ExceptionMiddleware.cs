using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Api.Middleware
{
    public sealed class ExceptionMiddleware
    {
        private const string ProblemJson = "application/problem+json";
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
                if (!ctx.Response.HasStarted)
                {
                    var errors = vex.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());

                    var problem = new ValidationProblemDetails(errors)
                    {
                        Title = "Validation Failed",
                        Status = StatusCodes.Status400BadRequest,
                        Instance = ctx.Request.Path,
                        Detail = "One or more validation errors occurred."
                    };
                    problem.Extensions["traceId"] = ctx.TraceIdentifier;

                    ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
                    ctx.Response.ContentType = ProblemJson;
                    await ctx.Response.WriteAsJsonAsync(problem);
                }
            }
            catch (UnauthorizedAccessException uex)
            {
                _logger.LogWarning(uex, "Unauthorized");
                if (!ctx.Response.HasStarted)
                {
                    var problem = NewProblem("Unauthorized", StatusCodes.Status401Unauthorized, ctx, "Authentication is required.");
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    ctx.Response.ContentType = ProblemJson;
                    await ctx.Response.WriteAsJsonAsync(problem);
                }
            }
            catch (KeyNotFoundException kex)
            {
                _logger.LogInformation(kex, "Not found");
                if (!ctx.Response.HasStarted)
                {
                    var detail = string.IsNullOrWhiteSpace(kex.Message) ? "The requested resource was not found." : kex.Message;
                    var problem = NewProblem("Not Found", StatusCodes.Status404NotFound, ctx, detail);
                    ctx.Response.StatusCode = StatusCodes.Status404NotFound;
                    ctx.Response.ContentType = ProblemJson;
                    await ctx.Response.WriteAsJsonAsync(problem);
                }
            }
            catch (BadHttpRequestException brex)
            {
                _logger.LogWarning(brex, "Bad request");
                if (!ctx.Response.HasStarted)
                {
                    var detail = string.IsNullOrWhiteSpace(brex.Message) ? "Malformed request." : brex.Message;
                    var problem = NewProblem("Bad Request", StatusCodes.Status400BadRequest, ctx, detail);
                    ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
                    ctx.Response.ContentType = ProblemJson;
                    await ctx.Response.WriteAsJsonAsync(problem);
                }
            }
            catch (OperationCanceledException ocex) when (ctx.RequestAborted.IsCancellationRequested)
            {
                _logger.LogInformation(ocex, "Request was canceled by the client");
                if (!ctx.Response.HasStarted)
                {
                    var problem = NewProblem("Client Closed Request", 499, ctx, "The client canceled the request.");
                    ctx.Response.StatusCode = 499;
                    ctx.Response.ContentType = ProblemJson;
                    await ctx.Response.WriteAsJsonAsync(problem);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                if (!ctx.Response.HasStarted)
                {
                    var problem = NewProblem("Server Error", StatusCodes.Status500InternalServerError, ctx, "An unexpected error occurred.");
                    ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    ctx.Response.ContentType = ProblemJson;
                    await ctx.Response.WriteAsJsonAsync(problem);
                }
            }
        }

        private static ProblemDetails NewProblem(string title, int status, HttpContext ctx, string detail)
        {
            var pd = new ProblemDetails
            {
                Title = title,
                Status = status,
                Detail = detail,
                Instance = ctx.Request.Path
            };
            pd.Extensions["traceId"] = ctx.TraceIdentifier;
            return pd;
        }
    }

    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiExceptionMiddleware(this IApplicationBuilder app)
            => app.UseMiddleware<ExceptionMiddleware>();
    }
}
