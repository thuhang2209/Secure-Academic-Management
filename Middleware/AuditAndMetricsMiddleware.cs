using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Claims;
using Secure_Academic_Management.Data;
using Secure_Academic_Management.Models;

namespace Secure_Academic_Management.Middleware;

public sealed class AuditAndMetricsMiddleware
{
    private static readonly ConcurrentDictionary<int, long> StatusCounts = new();
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditAndMetricsMiddleware> _logger;

    public AuditAndMetricsMiddleware(
        RequestDelegate next,
        ILogger<AuditAndMetricsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        if (IsExcludedPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            StatusCounts.AddOrUpdate(context.Response.StatusCode, 1, (_, count) => count + 1);

            try
            {
                dbContext.AuditLogs.Add(new AuditLog
                {
                    Action = $"{context.Request.Method} {context.Request.Path}",
                    HttpMethod = context.Request.Method,
                    Path = context.Request.Path.ToString(),
                    UserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier),
                    UserName = context.User.Identity?.Name,
                    IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = context.Request.Headers.UserAgent.ToString(),
                    StatusCode = context.Response.StatusCode,
                    DurationMilliseconds = stopwatch.ElapsedMilliseconds
                });
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to persist audit record for {Path}", context.Request.Path);
            }
        }
    }

    public static string RenderMetrics()
    {
        var lines = StatusCounts
            .OrderBy(item => item.Key)
            .Select(item => $"http_requests_total{{status_code=\"{item.Key}\"}} {item.Value}");
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static bool IsExcludedPath(PathString path)
    {
        return path.StartsWithSegments("/health")
            || path.StartsWithSegments("/metrics")
            || path.StartsWithSegments("/css")
            || path.StartsWithSegments("/js")
            || path.StartsWithSegments("/lib")
            || path.StartsWithSegments("/uploads");
    }
}
