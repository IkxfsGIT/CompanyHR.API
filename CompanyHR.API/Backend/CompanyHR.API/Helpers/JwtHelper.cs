using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CompanyHR.API.Helpers;

/// <summary>
/// Вспомогательный класс для генерации и валидации JWT-токенов
/// </summary>
public class JwtHelper
{
    private readonly JwtSettings _jwtSettings;

    public JwtHelper(JwtSettings jwtSettings)
    {
        _jwtSettings = jwtSettings;
    }

    /// <summary>
    /// Генерация access token
    /// </summary>
    public string GenerateAccessToken(int userId, string email, string role, IEnumerable<Claim>? additionalClaims = null)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        if (additionalClaims != null)
            claims.AddRange(additionalClaims);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddDays(_jwtSettings.AccessTokenExpireDays);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Генерация refresh token (случайная строка)
    /// </summary>
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    /// <summary>
    /// Валидация access token и получение ClaimsPrincipal
    /// </summary>
    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Получение срока действия токена
    /// </summary>
    public DateTime GetAccessTokenExpiration(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        return jwtToken.ValidTo;
    }

    /// <summary>
    /// Проверка, истёк ли токен
    /// </summary>
    public bool IsTokenExpired(string token)
    {
        var expiration = GetAccessTokenExpiration(token);
        return expiration < DateTime.UtcNow;
    }

    /// <summary>
    /// Извлечение userId из токена
    /// </summary>
    public int? GetUserIdFromToken(string token)
    {
        var principal = ValidateAccessToken(token);
        if (principal == null) return null;

        var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub) 
            ?? principal.FindFirst(ClaimTypes.NameIdentifier);
        
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            return userId;

        return null;
    }
}

/// <summary>
/// Настройки JWT (должны быть в appsettings.json)
/// </summary>
public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpireDays { get; set; } = 1;
    public int RefreshTokenExpireDays { get; set; } = 7;
}
