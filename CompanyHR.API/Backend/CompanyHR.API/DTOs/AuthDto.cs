using System.ComponentModel.DataAnnotations;

namespace CompanyHR.API.DTOs;

/// <summary>
/// DTO для ответа при успешной аутентификации
/// </summary>
public class AuthResponseDto
{
    /// <summary>
    /// Флаг успешности операции
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// JWT токен доступа
    /// </summary>
    public string? AccessToken { get; set; }
    
    /// <summary>
    /// Токен для обновления доступа
    /// </summary>
    public string? RefreshToken { get; set; }
    
    /// <summary>
    /// Время жизни токена в секундах
    /// </summary>
    public int ExpiresIn { get; set; }
    
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Email пользователя
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Роль пользователя
    /// </summary>
    public string? Role { get; set; }
    
    /// <summary>
    /// Имя пользователя (для отображения)
    /// </summary>
    public string? FullName { get; set; }
}

/// <summary>
/// DTO для запроса на обновление токена
/// </summary>
public class RefreshTokenRequestDto
{
    /// <summary>
    /// Токен обновления
    /// </summary>
    [Required(ErrorMessage = "Refresh token обязателен")]
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// DTO для ответа с информацией о пользователе
/// </summary>
public class UserInfoDto
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Email пользователя
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Роль пользователя
    /// </summary>
    public string Role { get; set; } = string.Empty;
    
    /// <summary>
    /// Имя
    /// </summary>
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// Фамилия
    /// </summary>
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// Полное имя
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";
    
    /// <summary>
    /// Должность
    /// </summary>
    public string? Position { get; set; }
    
    /// <summary>
    /// Отдел
    /// </summary>
    public string? Department { get; set; }
    
    /// <summary>
    /// Дата последнего входа
    /// </summary>
    public DateTime? LastLogin { get; set; }
}
