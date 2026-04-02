using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CompanyHR.API.Enums;

/// <summary>
/// Перечисление возможных статусов сотрудника
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EmployeeStatus
{
    /// <summary>
    /// Активный сотрудник (работает в настоящее время)
    /// </summary>
    [Display(Name = "Активен")]
    Active = 1,

    /// <summary>
    /// Неактивный сотрудник (уволен или не работает)
    /// </summary>
    [Display(Name = "Неактивен")]
    Inactive = 2,

    /// <summary>
    /// В отпуске
    /// </summary>
    [Display(Name = "В отпуске")]
    OnVacation = 3,

    /// <summary>
    /// На больничном
    /// </summary>
    [Display(Name = "На больничном")]
    OnSickLeave = 4,

    /// <summary>
    /// В декретном отпуске
    /// </summary>
    [Display(Name = "В декретном отпуске")]
    OnMaternityLeave = 5,

    /// <summary>
    /// Удалённая работа
    /// </summary>
    [Display(Name = "Удалённо")]
    Remote = 6,

    /// <summary>
    /// В испытательном сроке
    /// </summary>
    [Display(Name = "Испытательный срок")]
    Probation = 7,

    /// <summary>
    /// Частичная занятость
    /// </summary>
    [Display(Name = "Частичная занятость")]
    PartTime = 8
}

/// <summary>
/// Вспомогательный класс для работы с перечислением EmployeeStatus
/// </summary>
public static class EmployeeStatusExtensions
{
    /// <summary>
    /// Получение отображаемого названия статуса
    /// </summary>
    public static string GetDisplayName(this EmployeeStatus status)
    {
        var field = status.GetType().GetField(status.ToString());
        var attribute = field?.GetCustomAttributes(typeof(DisplayAttribute), false)
            .Cast<DisplayAttribute>()
            .FirstOrDefault();
        
        return attribute?.Name ?? status.ToString();
    }

    /// <summary>
    /// Получение всех доступных статусов в виде списка
    /// </summary>
    public static List<EmployeeStatus> GetAllStatuses()
    {
        return Enum.GetValues(typeof(EmployeeStatus))
            .Cast<EmployeeStatus>()
            .ToList();
    }

    /// <summary>
    /// Проверка, считается ли статус активным для работы
    /// </summary>
    public static bool IsActive(this EmployeeStatus status)
    {
        return status switch
        {
            EmployeeStatus.Active => true,
            EmployeeStatus.OnVacation => true,
            EmployeeStatus.OnSickLeave => true,
            EmployeeStatus.OnMaternityLeave => true,
            EmployeeStatus.Remote => true,
            EmployeeStatus.Probation => true,
            EmployeeStatus.PartTime => true,
            EmployeeStatus.Inactive => false,
            _ => false
        };
    }

    /// <summary>
    /// Получение цвета для отображения статуса в UI
    /// </summary>
    public static string GetStatusColor(this EmployeeStatus status)
    {
        return status switch
        {
            EmployeeStatus.Active => "green",
            EmployeeStatus.Inactive => "gray",
            EmployeeStatus.OnVacation => "blue",
            EmployeeStatus.OnSickLeave => "orange",
            EmployeeStatus.OnMaternityLeave => "purple",
            EmployeeStatus.Remote => "teal",
            EmployeeStatus.Probation => "yellow",
            EmployeeStatus.PartTime => "cyan",
            _ => "gray"
        };
    }

    /// <summary>
    /// Попытка преобразования строки в значение перечисления EmployeeStatus
    /// </summary>
    public static bool TryParse(string? value, out EmployeeStatus result)
    {
        result = EmployeeStatus.Inactive;
        
        if (string.IsNullOrWhiteSpace(value))
            return false;

        // Прямое преобразование
        if (Enum.TryParse(value, true, out EmployeeStatus parsed))
        {
            result = parsed;
            return true;
        }

        // Поиск по отображаемому имени
        foreach (EmployeeStatus status in GetAllStatuses())
        {
            if (status.GetDisplayName().Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                result = status;
                return true;
            }
        }

        return false;
    }
}
