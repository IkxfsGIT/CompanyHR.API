using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompanyHR.API.Models;

/// <summary>
/// Модель отдела компании
/// </summary>
[Table("Departments")]
public class Department
{
    [Key]
    public int DepartmentId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int? ManagerId { get; set; }

    [ForeignKey("ManagerId")]
    public Employee? Manager { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Навигационные свойства
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
