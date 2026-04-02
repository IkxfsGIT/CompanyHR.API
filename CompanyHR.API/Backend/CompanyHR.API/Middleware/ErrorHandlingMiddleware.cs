using System.Net;
using System.Text.Json;
using CompanyHR.API.DTOs.Responses;

namespace CompanyHR.API.Middleware;

/// <summary>
/// Middleware для глобальной обработки исключений
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Произошла ошибка при обработке запроса {Method} {Path}", 
            context.Request.Method, context.Request.Path);

        var response = context.Response;
        response.ContentType = "application/json";

        var (statusCode, message, details) = GetErrorDetails(exception);

        response.StatusCode = statusCode;

        var errorResponse = new ApiErrorResponse
        {
            StatusCode = statusCode,
            Message = message,
            Details = _env.IsDevelopment() ? details : null,
            Timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(json);
    }

    private (int statusCode, string message, string? details) GetErrorDetails(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, 
                "Недостаточно прав для выполнения операции", exception.StackTrace),
            
            InvalidOperationException => (StatusCodes.Status400BadRequest, 
                exception.Message, exception.StackTrace),
            
            KeyNotFoundException => (StatusCodes.Status404NotFound, 
                "Запрошенный ресурс не найден", exception.StackTrace),
            
            ArgumentException => (StatusCodes.Status400BadRequest, 
                "Некорректные параметры запроса", exception.StackTrace),
            
            _ => (StatusCodes.Status500InternalServerError, 
                "Внутренняя ошибка сервера", exception.StackTrace)
        };
    }
}
