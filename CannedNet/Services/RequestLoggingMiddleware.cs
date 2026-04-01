using System.Text;

namespace CannedNet.Services;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        
        var request = context.Request;
        var requestId = context.TraceIdentifier;
        
        _logger.LogInformation("[{RequestId}] {Method} {Path}{QueryString}", 
            requestId, request.Method, request.Path, request.QueryString);

        foreach (var header in request.Headers)
        {
            if (IsSensitiveHeader(header.Key))
                _logger.LogInformation("  [{RequestId}] {Header}: [REDACTED]", requestId, header.Key);
            else
                _logger.LogInformation("  [{RequestId}] {Header}: {Value}", requestId, header.Key, header.Value.ToString());
        }

        if (request.ContentLength > 0 && request.ContentLength < 10000)
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            
            if (!string.IsNullOrWhiteSpace(body))
                _logger.LogInformation("  [{RequestId}] Body: {Body}", requestId, body);
        }

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            _logger.LogInformation("[{RequestId}] -> {StatusCode} ({ElapsedMs}ms)", 
                requestId, context.Response.StatusCode, sw.ElapsedMilliseconds);
        }
    }

    private static bool IsSensitiveHeader(string headerName)
    {
        var sensitiveHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Authorization",
            "Cookie",
            "Set-Cookie",
            "X-Auth-Token",
            "X-Csrf-Token"
        };
        return sensitiveHeaders.Contains(headerName);
    }
}

public static class RequestLoggingExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
}
