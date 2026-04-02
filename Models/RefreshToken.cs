using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CompanyHR.API.Models;

/// <summary>
/// Модель refresh-токена
/// </summary>
[Table("RefreshTokens")]
public class RefreshToken
{
    [Key]
    public int RefreshTokenId { get; set; }

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    public DateTime ExpiryDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsRevoked { get; set; }

    public bool IsUsed { get; set; }
}
