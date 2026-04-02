using System.ComponentModel.DataAnnotations;

namespace CompanyHR.API.DTOs;

/// <summary>
/// DTO для регистрации нового пользователя
/// </summary>
public class RegisterDto
{
    /// <summary>
    /// Email пользователя
    /// </summary>
    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    [MaxLength(150, ErrorMessage = "Email не должен превышать 150 символов")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Пароль
    /// </summary>
    [Required(ErrorMessage = "Пароль обязателен")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$",
        ErrorMessage = "Пароль должен содержать заглавные, строчные буквы, цифры и спецсимволы")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Подтверждение пароля
    /// </summary>
    [Required(ErrorMessage = "Подтверждение пароля обязательно")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// Имя
    /// </summary>
    [Required(ErrorMessage = "Имя обязательно")]
    [MaxLength(100, ErrorMessage = "Имя не должно превышать 100 символов")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Фамилия
    /// </summary>
    [Required(ErrorMessage = "Фамилия обязательна")]
    [MaxLength(100, ErrorMessage = "Фамилия не должна превышать 100 символов")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Отчество (опционально)
    /// </summary>
    [MaxLength(100, ErrorMessage = "Отчество не должно превышать 100 символов")]
    public string? MiddleName { get; set; }

    /// <summary>
    /// Телефон
    /// </summary>
    [Phone(ErrorMessage = "Некорректный формат телефона")]
    [MaxLength(20, ErrorMessage = "Телефон не должен превышать 20 символов")]
    public string? Phone { get; set; }

    /// <summary>
    /// Дата рождения
    /// </summary>
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// Должность
    /// </summary>
    [MaxLength(100, ErrorMessage = "Должность не должна превышать 100 символов")]
    public string? Position { get; set; }

    /// <summary>
    /// Отдел
    /// </summary>
    [MaxLength(100, ErrorMessage = "Отдел не должен превышать 100 символов")]
    public string? Department { get; set; }

    /// <summary>
    /// Дата найма (по умолчанию текущая дата)
    /// </summary>
    [DataType(DataType.Date)]
    public DateTime HireDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Зарплата
    /// </summary>
    [Range(0, 9999999.99, ErrorMessage = "Некорректное значение зарплаты")]
    public decimal? Salary { get; set; }

    /// <summary>
    /// Адрес
    /// </summary>
    [MaxLength(200, ErrorMessage = "Адрес не должен превышать 200 символов")]
    public string? Address { get; set; }
}
