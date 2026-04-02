using System.Diagnostics;
using System.Text;

namespace CompanyHR.API.Middleware;

/// <summary>
/// Middleware для логирования HTTP-запросов и ответов
/// </summary>
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
        // Логирование запроса
        await LogRequest(context);

        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        using (var responseBody = new MemoryStream())
        {
            context.Response.Body = responseBody;

            await _next(context);

            stopwatch.Stop();

            // Логирование ответа
            await LogResponse(context, stopwatch.ElapsedMilliseconds);

            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private async Task LogRequest(HttpContext context)
    {
        context.Request.EnableBuffering();

        var buffer = new byte[Convert.ToInt32(context.Request.ContentLength)];
        await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
        var requestBody = Encoding.UTF8.GetString(buffer);

        context.Request.Body.Position = 0;

        _logger.LogInformation("HTTP Запрос: {Method} {Path} {QueryString} | Body: {Body}",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            requestBody);
    }

    private async Task LogResponse(HttpContext context, long elapsedMs)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        _logger.LogInformation("HTTP Ответ: {StatusCode} | Время выполнения: {ElapsedMs} мс | Body: {Body}",
            context.Response.StatusCode,
            elapsedMs,
            responseBody);
    }
}
