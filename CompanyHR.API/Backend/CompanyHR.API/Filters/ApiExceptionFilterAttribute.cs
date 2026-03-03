using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using CompanyHR.API.DTOs.Responses;

namespace CompanyHR.API.Filters;

/// <summary>
/// Фильтр исключений для централизованной обработки ошибок в API
/// </summary>
public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
{
    private readonly ILogger<ApiExceptionFilterAttribute> _logger;
    private readonly IWebHostEnvironment _env;

    public ApiExceptionFilterAttribute(ILogger<ApiExceptionFilterAttribute> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    /// <summary>
    /// Вызов при возникновении исключения
    /// </summary>
    public override void OnException(ExceptionContext context)
    {
        var exception = context.Exception;
        var request = context.HttpContext.Request;

        _logger.LogError(exception, "Ошибка при обработке запроса {Method} {Path}", 
            request.Method, request.Path);

        var response = CreateErrorResponse(exception);
        context.Result = new ObjectResult(response)
        {
            StatusCode = response.StatusCode
        };
        context.ExceptionHandled = true;
    }

    private ApiErrorResponse CreateErrorResponse(Exception exception)
    {
        var statusCode = GetStatusCode(exception);
        var message = GetUserFriendlyMessage(exception);
        var details = _env.IsDevelopment() ? exception.StackTrace : null;

        return new ApiErrorResponse
        {
            StatusCode = statusCode,
            Message = message,
            Details = details,
            Timestamp = DateTime.UtcNow
        };
    }

    private int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            ArgumentException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => "Недостаточно прав для выполнения операции",
            InvalidOperationException => exception.Message,
            KeyNotFoundException => "Запрошенный ресурс не найден",
            ArgumentException => "Некорректные параметры запроса",
            _ => "Произошла внутренняя ошибка сервера"
        };
    }
}

/// <summary>
/// DTO для ответа с ошибкой
/// </summary>
public class ApiErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; }
}
