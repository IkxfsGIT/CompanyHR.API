using System.Text;
using System.Text.Json.Serialization;
using CompanyHR.API.Data;
using CompanyHR.API.Middleware;
using CompanyHR.API.Models;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using CompanyHR.API.Filters;
using CompanyHR.API.Constants;
using CompanyHR.API.DTOs.Responses;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Converters;

var builder = WebApplication.CreateBuilder(args);

// ========================================================
// 1. Конфигурация сервисов (DI)
// ========================================================

// Добавление контроллеров с настройками JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Настройка формата дат
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        
        // Игнорирование циклических ссылок
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        
        // Формат дат
        options.JsonSerializerOptions.Converters.Add(new IsoDateTimeConverter
        {
            DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ"
        });
        
        // Использование camelCase для свойств (стандарт для JSON API)
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        
        // Игнорирование null значений (опционально)
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// ========== База данных ==========
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    // Режим разработки без реальной БД — используем InMemory
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("CompanyHR"));
    Console.WriteLine("Используется InMemoryDatabase (нет строки подключения)");
}
else
{
    // Реальная PostgreSQL
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}

// ========== AutoMapper ==========
builder.Services.AddAutoMapper(typeof(Program));

// ========== FluentValidation (регистрация валидаторов) ==========
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ========== Аутентификация JWT ==========
var jwtKey = builder.Configuration["Jwt:Key"] ?? "default_super_secret_key_32_chars_minimum_1234567890";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CompanyHR";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CompanyHRUsers";

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
            ClockSkew = TimeSpan.Zero // уменьшаем допустимый сдвиг времени
        };
    });

// ========== Автоматическая валидация DTO =============
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ========== Авторизация (роли) ==========
builder.Services.AddAuthorization();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddHostedService<EmployeeNotificationService>();

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

// Регистрация Unit of Work и репозиториев
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Регистрация сервисов
builder.Services.AddScoped<IPositionService, PositionService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<ICacheService, CacheService>();

builder.Services.AddDistributedMemoryCache();

// ========== Swagger (OpenAPI) ==========
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Company HR API",
        Version = "v1",
        Description = "API для управления сотрудниками компании",
        Contact = new OpenApiContact
        {
            Name = "HR Department",
            Email = "hr@company.com"
        }
    });

    // Добавляем поддержку JWT в Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Example: \"Bearer {token}\"",
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

    // Включение XML-комментариев (если есть)
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// ========== CORS ==========
builder.Services.AddCors(options =>
{
    options.AddPolicy("FlutterClient", policy =>
    {
        policy.WithOrigins(
                "http://localhost:8080",  // Flutter web
                "http://localhost:3000",  // Альтернативный порт
                "http://localhost:5000",  // Сам API
                "http://127.0.0.1:8080")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });

    builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutterApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:8080",      // Flutter web
                "http://localhost:5000",      // Сам API
                "http://localhost:3000")      // Альтернативный порт
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

    // Более широкая политика для разработки (опционально)
    options.AddPolicy("Development", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ========== Регистрация пользовательских сервисов ==========
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IAuthService, AuthService>();
// Добавьте другие ваши сервисы по аналогии

// ========== Фоновые службы (Background Services) ==========
// builder.Services.AddHostedService<EmployeeNotificationService>(); // раскомментируйте, когда создадите

// ========== Кэширование (Redis) ==========
// var redisConnection = builder.Configuration.GetConnectionString("Redis");
// if (!string.IsNullOrEmpty(redisConnection))
// {
//     builder.Services.AddStackExchangeRedisCache(options =>
//     {
//         options.Configuration = redisConnection;
//     });
// }

// ========== Логирование через Serilog (опционально) ==========
// builder.Host.UseSerilog((context, config) =>
// {
//     config.ReadFrom.Configuration(context.Configuration);
// });

// ========================================================
// 2. Сборка приложения
// ========================================================
var app = builder.Build();

// ========== Применение миграций при запуске (только для реальной БД) ==========
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

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Настройки JWT и Email
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));

// Если используете Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});
// Или для разработки можно использовать MemoryDistributedCache
// builder.Services.AddDistributedMemoryCache();

// ========================================================
// 3. Конвейер обработки запросов (Middleware)
// ========================================================

// Глобальная обработка ошибок (должна быть первой)
app.UseMiddleware<ErrorHandlingMiddleware>();

// Логирование запросов (опционально, если есть такой middleware)
app.UseMiddleware<RequestLoggingMiddleware>();

// Перенаправление на HTTPS (в production обязательно)
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

// Swagger только в разработке (можно и в production, если нужно)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Company HR API v1");
        c.RoutePrefix = "swagger"; // http://localhost:5000/swagger
    });
}

// CORS — выбрать подходящую политику
if (app.Environment.IsDevelopment())
{
    app.UseCors("Development");
}
else
{
    app.UseCors("FlutterClient");
}

app.UseCors("AllowFlutterApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
