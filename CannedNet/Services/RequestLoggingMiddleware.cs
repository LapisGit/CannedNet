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
        
        string body = "";
        if (request.ContentLength > 0 && request.ContentLength < 10000)
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
        }

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            
            var headers = string.Join("\n  ", request.Headers
                .Where(h => !IsSensitiveHeader(h.Key))
                .Select(h => $"{h.Key}: {h.Value}"));
            
            var sensitiveHeaders = string.Join("\n  ", request.Headers
                .Where(h => IsSensitiveHeader(h.Key))
                .Select(h => $"{h.Key}: [REDACTED]"));
            
            var headerStr = string.IsNullOrEmpty(sensitiveHeaders) ? headers : $"{headers}\n  {sensitiveHeaders}";
            
            if (!string.IsNullOrWhiteSpace(body))
                _logger.LogInformation("[{RequestId}] {Method} {Path}{QueryString}\n  {HeaderStr}\n  Body: {Body}", 
                    requestId, request.Method, request.Path, request.QueryString, headerStr, body);
            else
                _logger.LogInformation("[{RequestId}] {Method} {Path}{QueryString}\n  {HeaderStr}", 
                    requestId, request.Method, request.Path, request.QueryString, headerStr);
        }
    }

    private static bool IsSensitiveHeader(string headerName)
    {
        var sensitiveHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { };
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
