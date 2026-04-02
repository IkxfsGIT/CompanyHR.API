using System.ComponentModel.DataAnnotations;

namespace CompanyHR.API.DTOs;

/// <summary>
/// DTO для отображения информации о должности
/// </summary>
public class PositionDto
{
    /// <summary>
    /// Идентификатор должности
    /// </summary>
    public int PositionId { get; set; }
    
    /// <summary>
    /// Название должности
    /// </summary>
    public string PositionName { get; set; } = string.Empty;
    
    /// <summary>
    /// Описание должности
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Уровень должности (1-10)
    /// </summary>
    public int Level { get; set; }
    
    /// <summary>
    /// Количество сотрудников на этой должности
    /// </summary>
    public int EmployeeCount { get; set; }
    
    /// <summary>
    /// Минимальная зарплата по должности
    /// </summary>
    public decimal? MinSalary { get; set; }
    
    /// <summary>
    /// Максимальная зарплата по должности
    /// </summary>
    public decimal? MaxSalary { get; set; }
    
    /// <summary>
    /// Средняя зарплата по должности
    /// </summary>
    public decimal? AverageSalary { get; set; }
    
    /// <summary>
    /// Дата создания записи
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Дата последнего обновления
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO для создания новой должности
/// </summary>
public class CreatePositionDto
{
    /// <summary>
    /// Название должности
    /// </summary>
    [Required(ErrorMessage = "Название должности обязательно")]
    [MaxLength(100, ErrorMessage = "Название должности не должно превышать 100 символов")]
    public string PositionName { get; set; } = string.Empty;
    
    /// <summary>
    /// Описание должности
    /// </summary>
    [MaxLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
    public string? Description { get; set; }
    
    /// <summary>
    /// Уровень должности
    /// </summary>
    [Range(1, 10, ErrorMessage = "Уровень должен быть от 1 до 10")]
    public int Level { get; set; } = 1;
}

/// <summary>
/// DTO для обновления должности
/// </summary>
public class UpdatePositionDto
{
    /// <summary>
    /// Название должности
    /// </summary>
    [MaxLength(100, ErrorMessage = "Название должности не должно превышать 100 символов")]
    public string? PositionName { get; set; }
    
    /// <summary>
    /// Описание должности
    /// </summary>
    [MaxLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
    public string? Description { get; set; }
    
    /// <summary>
    /// Уровень должности
    /// </summary>
    [Range(1, 10, ErrorMessage = "Уровень должен быть от 1 до 10")]
    public int? Level { get; set; }
}
