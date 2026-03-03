using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompanyHR.API.Models;

/// <summary>
/// Модель должности
/// </summary>
[Table("Positions")]
public class Position
{
    [Key]
    public int PositionId { get; set; }

    [Required]
    [MaxLength(100)]
    public string PositionName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int Level { get; set; } = 1;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Навигационные свойства
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
