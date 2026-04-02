using System.ComponentModel.DataAnnotations;

namespace CompanyHR.API.DTOs;

/// <summary>
/// DTO для отображения информации об отделе
/// </summary>
public class DepartmentDto
{
    /// <summary>
    /// Название отдела
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Количество сотрудников в отделе
    /// </summary>
    public int EmployeeCount { get; set; }
    
    /// <summary>
    /// Количество активных сотрудников
    /// </summary>
    public int ActiveEmployeeCount { get; set; }
    
    /// <summary>
    /// Средняя зарплата по отделу
    /// </summary>
    public decimal AverageSalary { get; set; }
    
    /// <summary>
    /// Руководитель отдела (если есть)
    /// </summary>
    public string? ManagerName { get; set; }
}

/// <summary>
/// DTO для создания нового отдела
/// </summary>
public class CreateDepartmentDto
{
    /// <summary>
    /// Название отдела
    /// </summary>
    [Required(ErrorMessage = "Название отдела обязательно")]
    [MaxLength(100, ErrorMessage = "Название отдела не должно превышать 100 символов")]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Описание отдела
    /// </summary>
    [MaxLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
    public string? Description { get; set; }
}

/// <summary>
/// DTO для обновления отдела
/// </summary>
public class UpdateDepartmentDto
{
    /// <summary>
    /// Название отдела
    /// </summary>
    [MaxLength(100, ErrorMessage = "Название отдела не должно превышать 100 символов")]
    public string? Name { get; set; }
    
    /// <summary>
    /// Описание отдела
    /// </summary>
    [MaxLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
    public string? Description { get; set; }
}

/// <summary>
/// DTO для статистики по отделу
/// </summary>
public class DepartmentStatisticsDto
{
    /// <summary>
    /// Название отдела
    /// </summary>
    public string DepartmentName { get; set; } = string.Empty;
    
    /// <summary>
    /// Общее количество сотрудников
    /// </summary>
    public int TotalEmployees { get; set; }
    
    /// <summary>
    /// Количество сотрудников по должностям
    /// </summary>
    public Dictionary<string, int> EmployeesByPosition { get; set; } = new();
    
    /// <summary>
    /// Распределение по стажу работы
    /// </summary>
    public Dictionary<string, int> TenureDistribution { get; set; } = new();
    
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
