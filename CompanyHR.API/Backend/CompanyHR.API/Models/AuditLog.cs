using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompanyHR.API.Models;

/// <summary>
/// Модель для логирования действий пользователей в системе
/// </summary>
[Table("AuditLogs")]
public class AuditLog
{
    [Key]
    public int AuditLogId { get; set; }

    [Required]
    [MaxLength(50)]
    public string TableName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Action { get; set; } = string.Empty; // INSERT, UPDATE, DELETE

    public int? RecordId { get; set; }

    [MaxLength(100)]
    public string? UserId { get; set; }

    [MaxLength(150)]
    public string? UserEmail { get; set; }

    [Column(TypeName = "jsonb")]
    public string? OldValues { get; set; }

    [Column(TypeName = "jsonb")]
    public string? NewValues { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string? IpAddress { get; set; }
}
