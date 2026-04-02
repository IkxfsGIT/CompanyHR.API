using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using CompanyHR.API.Data;
using CompanyHR.API.DTOs;
using CompanyHR.API.Helpers;
using CompanyHR.API.Models;
using Microsoft.Extensions.Options;

namespace CompanyHR.API.Services;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterDto dto);
    Task<AuthResult> LoginAsync(LoginDto dto);
    Task LogoutAsync(string refreshToken);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
}

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly JwtHelper _jwtHelper;
    private readonly ILogger<AuthService> _logger;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        ApplicationDbContext context,
        JwtHelper jwtHelper,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger)
    {
        _context = context;
        _jwtHelper = jwtHelper;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<AuthResult> RegisterAsync(RegisterDto dto)
    {
        // Проверяем, существует ли пользователь с таким email
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (existingUser != null)
            throw new InvalidOperationException("Пользователь с таким email уже существует");

        // Создаём запись о сотруднике (Employee), если её ещё нет
        // В реальном проекте может быть логика, что сотрудник уже существует в системе,
        // и регистрация привязывается к нему. Упростим: создаём нового сотрудника.
        var employee = new Employee
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Position = dto.Position,
            HireDate = dto.HireDate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync(); // чтобы получить Id

        // Хешируем пароль
        var passwordHash = PasswordHelper.HashPassword(dto.Password);

        // Создаём пользователя
        var user = new User
        {
            Email = dto.Email,
            PasswordHash = passwordHash,
            Role = "Employee", // по умолчанию
            IsActive = true,
            EmployeeId = employee.EmployeeId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Генерируем токены
        var accessToken = _jwtHelper.GenerateToken(user.Id, user.Email, user.Role);
        var refreshToken = GenerateRefreshToken();

        // Сохраняем refresh token в БД (создаём отдельную таблицу RefreshTokens)
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpireDays),
            CreatedAt = DateTime.UtcNow
        };
        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Пользователь {Email} успешно зарегистрирован", user.Email);

        return new AuthResult
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = (int)TimeSpan.FromDays(_jwtSettings.AccessTokenExpireDays).TotalSeconds,
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role
        };
    }

    public async Task<AuthResult> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("Неверный email или пароль");

        if (!PasswordHelper.VerifyPassword(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Неверный email или пароль");

        // Обновляем дату последнего входа
        user.LastLogin = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Генерируем токены
        var accessToken = _jwtHelper.GenerateToken(user.Id, user.Email, user.Role);
        var refreshToken = GenerateRefreshToken();

        // Сохраняем refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpireDays),
            CreatedAt = DateTime.UtcNow
        };
        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Пользователь {Email} вошёл в систему", user.Email);

        return new AuthResult
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = (int)TimeSpan.FromDays(_jwtSettings.AccessTokenExpireDays).TotalSeconds,
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role
        };
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);
        if (token != null)
        {
            _context.RefreshTokens.Remove(token);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Пользователь вышел из системы, refresh token удалён");
        }
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (token == null || token.ExpiryDate < DateTime.UtcNow || !token.User.IsActive)
            throw new UnauthorizedAccessException("Недействительный или просроченный refresh token");

        // Генерируем новый access token
        var user = token.User;
        var newAccessToken = _jwtHelper.GenerateToken(user.Id, user.Email, user.Role);
        var newRefreshToken = GenerateRefreshToken();

        // Обновляем refresh token в БД (можно перезаписать или создать новый)
        // Удаляем старый и добавляем новый
        _context.RefreshTokens.Remove(token);
        var newRefreshTokenEntity = new RefreshToken
        {
            Token = newRefreshToken,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpireDays),
            CreatedAt = DateTime.UtcNow
        };
        _context.RefreshTokens.Add(newRefreshTokenEntity);
        await _context.SaveChangesAsync();

        return new AuthResult
        {
            Success = true,
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = (int)TimeSpan.FromDays(_jwtSettings.AccessTokenExpireDays).TotalSeconds,
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role
        };
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}

// DTO для результата аутентификации
public class AuthResult
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public int UserId { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
}

// Настройки JWT (добавить в appsettings.json)
public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpireDays { get; set; } = 1;
    public int RefreshTokenExpireDays { get; set; } = 7;
}
