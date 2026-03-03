using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CompanyHR.API.Enums;

namespace CompanyHR.API.Models;

/// <summary>
/// Модель сотрудника компании
/// </summary>
[Table("Employees")]
public class Employee
{
    [Key]
    public int EmployeeId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? MiddleName { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [MaxLength(20)]
    public string? Phone { get; set; }

    public DateTime? BirthDate { get; set; }

    [Required]
    public DateTime HireDate { get; set; }

    [MaxLength(100)]
    public string? Position { get; set; }

    public int? PositionId { get; set; }

    [ForeignKey("PositionId")]
    public Position? PositionNavigation { get; set; }

    [MaxLength(100)]
    public string? Department { get; set; }

    public int? DepartmentId { get; set; }

    [ForeignKey("DepartmentId")]
    public Department? DepartmentNavigation { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Salary { get; set; }

    [MaxLength(200)]
    public string? Address { get; set; }

    [MaxLength(500)]
    public string? PhotoUrl { get; set; }

    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Навигационные свойства
    public User? User { get; set; }
}
