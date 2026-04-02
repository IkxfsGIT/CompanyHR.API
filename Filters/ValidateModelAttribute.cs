using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using CompanyHR.API.DTOs.Responses;

namespace CompanyHR.API.Filters;

/// <summary>
/// Фильтр для автоматической проверки состояния модели
/// </summary>
public class ValidateModelAttribute : ActionFilterAttribute
{
    private readonly bool _logErrors;

    /// <summary>
    /// Конструктор с возможностью логирования ошибок
    /// </summary>
    public ValidateModelAttribute(bool logErrors = true)
    {
        _logErrors = logErrors;
    }

    /// <summary>
    /// Выполняется перед выполнением действия
    /// </summary>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors.Select(er => new
                {
                    Field = e.Key,
                    Error = er.ErrorMessage
                }))
                .ToList();

            if (_logErrors)
            {
                var logger = context.HttpContext.RequestServices.GetService<ILogger<ValidateModelAttribute>>();
                logger?.LogWarning("Ошибка валидации для {Controller}.{Action}: {Errors}",
                    context.Controller.GetType().Name,
                    context.ActionDescriptor.DisplayName,
                    string.Join("; ", errors.Select(e => $"{e.Field}: {e.Error}")));
            }

            var response = ApiResponse<object>.ErrorResponse(
                "Ошибка валидации входных данных",
                errors.Select(e => $"{e.Field}: {e.Error}").ToList()
            );

            context.Result = new BadRequestObjectResult(response);
        }
    }
}
