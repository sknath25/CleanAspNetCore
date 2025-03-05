using Serilog;
using Serilog.Context;

namespace Api.Configuration.Correlation;

public record CorrelationId(string Value)
{
    public override string ToString()
    {
        return Value;
    }
}

public interface ICorrelationService
{
    CorrelationId GetCorrelationId();
    void SetCorrelationId(CorrelationId cid);
}

public class CorrelationService : ICorrelationService, IDisposable
{
    private IDisposable? logContext;
    private CorrelationId? cid;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            logContext?.Dispose();
            logContext = null;
        }
    }

    public CorrelationId GetCorrelationId()
    {
        return cid ?? new CorrelationId(Guid.NewGuid().ToString());
    }

    public void SetCorrelationId(CorrelationId cid)
    {
        this.cid = cid;
        logContext = LogContext.PushProperty(nameof(CorrelationId), cid);
    }
}

public class CorrelationHeaderMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var expectedKeys = new string[]
        {
            "CID",
            "CorrelationID",
            "Correlation-ID",
            "X-Correlation-ID"
        };

        CorrelationId? correlationId = null;

        var headerKeyName = expectedKeys[0];

        foreach (var keyName in expectedKeys)
        {
            var found = false;

            foreach (var header in context.Request.Headers)
            {
                if (header.Key.Equals(keyName, StringComparison.OrdinalIgnoreCase))
                {
                    headerKeyName = header.Key;
                    correlationId = new CorrelationId(header.Value.FirstOrDefault() ?? string.Empty);
                    found = true;
                    break;
                }
            }

            if (found) break;
        }

        if (string.IsNullOrWhiteSpace(correlationId?.Value))
        {
            correlationId = new CorrelationId(Guid.NewGuid().ToString());
        }

        var cidService = context.RequestServices.GetService<ICorrelationService>();

        cidService?.SetCorrelationId(correlationId);

        context.Response.Headers[headerKeyName] = correlationId.Value;

        await next(context);
    }
}

public static class CorrelationExtensions
{
    public static IApplicationBuilder UseCorrelationHeaderMiddleware(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationHeaderMiddleware>();
    }

    public static IServiceCollection AddCorrelationLogging(this IServiceCollection services)
    {
        return services.AddScoped<ICorrelationService, CorrelationService>();
    }
}
