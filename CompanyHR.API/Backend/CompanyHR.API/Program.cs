using System.Text;
using System.Text.Json.Serialization;
using CompanyHR.API.Constants;
using CompanyHR.API.Data;
using CompanyHR.API.Extensions;
using CompanyHR.API.Filters;
using CompanyHR.API.Helpers;
using CompanyHR.API.Middleware;
using CompanyHR.API.Services;
using CompanyHR.API.Repositories;
using CompanyHR.API.Repositories.Interfaces;
using CompanyHR.API.BackgroundServices;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ========================================================
// 1. Регистрация сервисов в контейнере внедрения зависимостей
// ========================================================

// ----- Контроллеры, фильтры, сериализация -----
builder.Services.AddControllers(options =>
{
    // Глобальные фильтры
    options.Filters.Add<ApiExceptionFilterAttribute>();
    options.Filters.Add<ValidateModelAttribute>();
})
.AddJsonOptions(options =>
{
    // Настройка сериализации JSON
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
})
.AddFluentValidationAutoValidation();   // Автоматическая валидация через FluentValidation

// ----- Версионирование API -----
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// ----- База данных -----
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    // Режим разработки без реальной БД — использование InMemory
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("CompanyHR"));
    Console.WriteLine("Используется InMemoryDatabase (строка подключения отсутствует)");
}
else
{
    // Подключение к PostgreSQL
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}

// ----- AutoMapper -----
builder.Services.AddAutoMapper(typeof(Program));

// ----- FluentValidation (регистрация валидаторов) -----
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ----- Аутентификация JWT -----
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<JwtHelper>();

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
var jwtKey = jwtSettings?.Key ?? "default_super_secret_key_32_chars_minimum_1234567890";
var jwtIssuer = jwtSettings?.Issuer ?? "CompanyHR";
var jwtAudience = jwtSettings?.Audience ?? "CompanyHRUsers";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero   // Уменьшение допустимого сдвига времени
        };
    });

// ----- Авторизация (политики на основе ролей) -----
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(ApplicationConstants.Policies.RequireAdmin,
        policy => policy.RequireRole(Roles.Admin));
    options.AddPolicy(ApplicationConstants.Policies.RequireHR,
        policy => policy.RequireRole(Roles.HR));
    options.AddPolicy(ApplicationConstants.Policies.RequireAdminOrHR,
        policy => policy.RequireRole(Roles.Admin, Roles.HR));
    options.AddPolicy(ApplicationConstants.Policies.RequireManager,
        policy => policy.RequireRole(Roles.Manager));
});

// ----- Репозитории и Unit of Work -----
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// ----- Бизнес-сервисы -----
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPositionService, PositionService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// ----- Сервисы кэширования -----
builder.Services.AddScoped<ICacheService, CacheService>();
// Выбор провайдера кэша: Redis (если настроен) или In-Memory
var redisConnection = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

// ----- Фоновые службы -----
builder.Services.AddHostedService<EmployeeNotificationService>();

// ----- Настройки для Email и JWT (через IOptions) -----
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));

// ----- Swagger (OpenAPI) -----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = ApplicationConstants.ApplicationName,
        Version = ApplicationConstants.ApplicationVersion,
        Description = "API для управления сотрудниками компании",
        Contact = new OpenApiContact
        {
            Name = "HR Department",
            Email = "hr@company.com"
        }
    });

    // Добавление поддержки JWT в Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Пример: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Включение XML-комментариев (если файл существует)
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// ----- CORS (политики доступа) -----
builder.Services.AddCors(options =>
{
    // Политика для разработки (разрешены все источники)
    options.AddPolicy("Development", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    // Политика для Flutter-клиента (ограниченные источники)
    options.AddPolicy("FlutterClient", policy =>
    {
        policy.WithOrigins(
                "http://localhost:8080",
                "http://localhost:3000",
                "http://localhost:5000",
                "http://127.0.0.1:8080")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });

    // Политика для продакшена (можно дополнить)
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://company-hr-frontend.vercel.app")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ========================================================
// 2. Сборка приложения
// ========================================================
var app = builder.Build();

// ========== Применение миграций (только для реальной БД) ==========
if (!string.IsNullOrEmpty(connectionString))
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        try
        {
            dbContext.Database.Migrate();
            Console.WriteLine("Миграции применены успешно");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка применения миграций: {ex.Message}");
        }
    }
}

// ========================================================
// 3. Конфигурация конвейера обработки запросов (Middleware)
// ========================================================

// Глобальная обработка ошибок (должна быть первой)
app.UseMiddleware<ErrorHandlingMiddleware>();

// Логирование запросов и ответов (опционально)
app.UseMiddleware<RequestLoggingMiddleware>();

// Перенаправление на HTTPS (только в продакшене)
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

// Swagger UI (только в разработке)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Company HR API v1");
        c.RoutePrefix = "swagger";
    });
}

// Выбор CORS-политики в зависимости от окружения
if (app.Environment.IsDevelopment())
{
    app.UseCors("Development");
}
else
{
    app.UseCors("Production");
}

// Аутентификация и авторизация
app.UseAuthentication();
app.UseAuthorization();

// Маршрутизация контроллеров
app.MapControllers();

// ========================================================
// 4. Запуск приложения
// ========================================================
app.Run();
