using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CompanyHR.API.Enums;

/// <summary>
/// Перечисление предопределённых отделов компании
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Department
{
    /// <summary>
    /// Отдел информационных технологий
    /// </summary>
    [Display(Name = "Информационные технологии")]
    IT = 1,

    /// <summary>
    /// Отдел кадров
    /// </summary>
    [Display(Name = "Управление персоналом")]
    HR = 2,

    /// <summary>
    /// Бухгалтерия
    /// </summary>
    [Display(Name = "Бухгалтерия")]
    Accounting = 3,

    /// <summary>
    /// Финансовый отдел
    /// </summary>
    [Display(Name = "Финансы")]
    Finance = 4,

    /// <summary>
    /// Отдел продаж
    /// </summary>
    [Display(Name = "Продажи")]
    Sales = 5,

    /// <summary>
    /// Маркетинговый отдел
    /// </summary>
    [Display(Name = "Маркетинг")]
    Marketing = 6,

    /// <summary>
    /// Производственный отдел
    /// </summary>
    [Display(Name = "Производство")]
    Production = 7,

    /// <summary>
    /// Отдел логистики
    /// </summary>
    [Display(Name = "Логистика")]
    Logistics = 8,

    /// <summary>
    /// Юридический отдел
    /// </summary>
    [Display(Name = "Юридический отдел")]
    Legal = 9,

    /// <summary>
    /// Администрация
    /// </summary>
    [Display(Name = "Администрация")]
    Administration = 10,

    /// <summary>
    /// Другое (для отделов, не входящих в предопределённый список)
    /// </summary>
    [Display(Name = "Другое")]
    Other = 99
}

/// <summary>
/// Вспомогательный класс для работы с перечислением Department
/// </summary>
public static class DepartmentExtensions
{
    /// <summary>
    /// Получение отображаемого названия отдела
    /// </summary>
    public static string GetDisplayName(this Department department)
    {
        var field = department.GetType().GetField(department.ToString());
        var attribute = field?.GetCustomAttributes(typeof(DisplayAttribute), false)
            .Cast<DisplayAttribute>()
            .FirstOrDefault();
        
        return attribute?.Name ?? department.ToString();
    }

    /// <summary>
    /// Получение всех доступных отделов в виде списка
    /// </summary>
    public static List<Department> GetAllDepartments()
    {
        return Enum.GetValues(typeof(Department))
            .Cast<Department>()
            .ToList();
    }

    /// <summary>
    /// Попытка преобразования строки в значение перечисления Department
    /// </summary>
    public static bool TryParse(string? value, out Department result)
    {
        result = Department.Other;
        
        if (string.IsNullOrWhiteSpace(value))
            return false;

        // Прямое преобразование
        if (Enum.TryParse(value, true, out Department parsed))
        {
            result = parsed;
            return true;
        }

        // Поиск по отображаемому имени
        foreach (Department dept in GetAllDepartments())
        {
            if (dept.GetDisplayName().Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                result = dept;
                return true;
            }
        }

        return false;
    }
}
