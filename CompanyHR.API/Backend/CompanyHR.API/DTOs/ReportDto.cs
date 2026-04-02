using System.ComponentModel.DataAnnotations;

namespace CompanyHR.API.DTOs;

/// <summary>
/// DTO для параметров отчёта
/// </summary>
public class ReportRequestDto
{
    /// <summary>
    /// Тип отчёта
    /// </summary>
    [Required(ErrorMessage = "Тип отчёта обязателен")]
    public string ReportType { get; set; } = string.Empty;
    
    /// <summary>
    /// Начальная дата периода
    /// </summary>
    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// Конечная дата периода
    /// </summary>
    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Формат отчёта (json, excel, pdf)
    /// </summary>
    public string Format { get; set; } = "json";
    
    /// <summary>
    /// Фильтр по отделу
    /// </summary>
    public string? Department { get; set; }
    
    /// <summary>
    /// Фильтр по должности
    /// </summary>
    public string? Position { get; set; }
    
    /// <summary>
    /// Включать ли только активных сотрудников
    /// </summary>
    public bool OnlyActive { get; set; } = true;
}

/// <summary>
/// DTO для сводного отчёта по сотрудникам
/// </summary>
public class EmployeeSummaryReportDto
{
    /// <summary>
    /// Дата формирования отчёта
    /// </summary>
    public DateTime GeneratedAt { get; set; }
    
    /// <summary>
    /// Общее количество сотрудников
    /// </summary>
    public int TotalEmployees { get; set; }
    
    /// <summary>
    /// Количество активных сотрудников
    /// </summary>
    public int ActiveEmployees { get; set; }
    
    /// <summary>
    /// Количество сотрудников в отпуске/неактивных
    /// </summary>
    public int InactiveEmployees { get; set; }
    
    /// <summary>
    /// Распределение по отделам
    /// </summary>
    public Dictionary<string, int> EmployeesByDepartment { get; set; } = new();
    
    /// <summary>
    /// Распределение по должностям
    /// </summary>
    public Dictionary<string, int> EmployeesByPosition { get; set; } = new();
    
    /// <summary>
    /// Средняя зарплата по компании
    /// </summary>
    public decimal AverageSalary { get; set; }
    
    /// <summary>
    /// Общий фонд оплаты труда
    /// </summary>
    public decimal TotalSalaryBudget { get; set; }
    
    /// <summary>
    /// Количество новых сотрудников за период
    /// </summary>
    public int NewHires { get; set; }
    
    /// <summary>
    /// Количество уволившихся за период
    /// </summary>
    public int Terminations { get; set; }
}

/// <summary>
/// DTO для отчёта по зарплатам
/// </summary>
public class SalaryReportDto
{
    /// <summary>
    /// Период отчёта
    /// </summary>
    public string Period { get; set; } = string.Empty;
    
    /// <summary>
    /// Сводка по зарплатам
    /// </summary>
    public SalarySummaryDto Summary { get; set; } = new();
    
    /// <summary>
    /// Распределение зарплат по отделам
    /// </summary>
    public List<DepartmentSalaryDto> ByDepartment { get; set; } = new();
    
    /// <summary>
    /// Распределение зарплат по должностям
    /// </summary>
    public List<PositionSalaryDto> ByPosition { get; set; } = new();
}

/// <summary>
/// DTO для сводки по зарплатам
/// </summary>
public class SalarySummaryDto
{
    /// <summary>
    /// Минимальная зарплата
    /// </summary>
    public decimal MinSalary { get; set; }
    
    /// <summary>
    /// Максимальная зарплата
    /// </summary>
    public decimal MaxSalary { get; set; }
    
    /// <summary>
    /// Средняя зарплата
    /// </summary>
    public decimal AverageSalary { get; set; }
    
    /// <summary>
    /// Медианная зарплата
    /// </summary>
    public decimal MedianSalary { get; set; }
    
    /// <summary>
    /// Общий фонд оплаты труда
    /// </summary>
    public decimal TotalSalaryBudget { get; set; }
    
    /// <summary>
    /// Количество сотрудников, получающих зарплату
    /// </summary>
    public int EmployeesWithSalary { get; set; }
}

/// <summary>
/// DTO для зарплат по отделам
/// </summary>
public class DepartmentSalaryDto
{
    /// <summary>
    /// Название отдела
    /// </summary>
    public string Department { get; set; } = string.Empty;
    
    /// <summary>
    /// Количество сотрудников
    /// </summary>
    public int EmployeeCount { get; set; }
    
    /// <summary>
    /// Средняя зарплата
    /// </summary>
    public decimal AverageSalary { get; set; }
    
    /// <summary>
    /// Общий фонд по отделу
    /// </summary>
    public decimal TotalSalary { get; set; }
    
    /// <summary>
    /// Доля в общем фонде (%)
    /// </summary>
    public decimal PercentageOfTotal { get; set; }
}

/// <summary>
/// DTO для зарплат по должностям
/// </summary>
public class PositionSalaryDto
{
    /// <summary>
    /// Название должности
    /// </summary>
    public string Position { get; set; } = string.Empty;
    
    /// <summary>
    /// Количество сотрудников
    /// </summary>
    public int EmployeeCount { get; set; }
    
    /// <summary>
    /// Средняя зарплата
    /// </summary>
    public decimal AverageSalary { get; set; }
    
    /// <summary>
    /// Минимальная зарплата
    /// </summary>
    public decimal MinSalary { get; set; }
    
    /// <summary>
    /// Максимальная зарплата
    /// </summary>
    public decimal MaxSalary { get; set; }
}

/// <summary>
/// DTO для отчёта по дням рождения
/// </summary>
public class BirthdayReportDto
{
    /// <summary>
    /// Месяц
    /// </summary>
    public int Month { get; set; }
    
    /// <summary>
    /// Название месяца
    /// </summary>
    public string MonthName { get; set; } = string.Empty;
    
    /// <summary>
    /// Список именинников
    /// </summary>
    public List<BirthdayEmployeeDto> Employees { get; set; } = new();
    
    /// <summary>
    /// Количество именинников
    /// </summary>
    public int Count => Employees.Count;
}

/// <summary>
/// DTO для сотрудника в отчёте по дням рождения
/// </summary>
public class BirthdayEmployeeDto
{
    /// <summary>
    /// Идентификатор сотрудника
    /// </summary>
    public int EmployeeId { get; set; }
    
    /// <summary>
    /// Полное имя
    /// </summary>
    public string FullName { get; set; } = string.Empty;
    
    /// <summary>
    /// Дата рождения
    /// </summary>
    public DateTime BirthDate { get; set; }
    
    /// <summary>
    /// День недели рождения в этом году
    /// </summary>
    public string DayOfWeek { get; set; } = string.Empty;
    
    /// <summary>
    /// Должность
    /// </summary>
    public string? Position { get; set; }
    
    /// <summary>
    /// Отдел
    /// </summary>
    public string? Department { get; set; }
    
    /// <summary>
    /// Email
    /// </summary>
    public string? Email { get; set; }
}
