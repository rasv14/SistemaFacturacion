using System.Diagnostics;

namespace Comercial360.Api.Infrastructure;

public sealed class CorrelationIdMiddleware(
    RequestDelegate next,
    ILogger<CorrelationIdMiddleware> logger)
{
    public const string ItemKey = "CorrelationId";

    public async Task InvokeAsync(HttpContext context)
    {
        var suppliedId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();
        var correlationId = Guid.TryParse(suppliedId, out var parsedId)
            ? parsedId.ToString("D")
            : Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("D");

        context.Items[ItemKey] = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Correlation-ID"] = correlationId;
            return Task.CompletedTask;
        });

        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await next(context);
        }
    }
}
