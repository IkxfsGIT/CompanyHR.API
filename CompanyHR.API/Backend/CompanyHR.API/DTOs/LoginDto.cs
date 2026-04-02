using System.ComponentModel.DataAnnotations;

namespace CompanyHR.API.DTOs;

/// <summary>
/// DTO для входа пользователя в систему
/// </summary>
public class LoginDto
{
    /// <summary>
    /// Email пользователя
    /// </summary>
    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    [MaxLength(150, ErrorMessage = "Email не должен превышать 150 символов")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Пароль пользователя
    /// </summary>
    [Required(ErrorMessage = "Пароль обязателен")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Запомнить ли пользователя (для увеличения срока жизни токена)
    /// </summary>
    public bool RememberMe { get; set; }
}

/// <summary>
/// DTO для ответа после успешного входа
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// Флаг успешности входа
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Сообщение о результате
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// JWT токен доступа
    /// </summary>
    public string? AccessToken { get; set; }
    
    /// <summary>
    /// Токен для обновления
    /// </summary>
    public string? RefreshToken { get; set; }
    
    /// <summary>
    /// Время истечения токена
    /// </summary>
    public DateTime Expiration { get; set; }
    
    /// <summary>
    /// Информация о пользователе
    /// </summary>
    public UserInfoDto? UserInfo { get; set; }
}
