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

var builder = WebApplication.CreateBuilder(args);

// ========================================================
// 1. Конфигурация сервисов (DI)
// ========================================================

// Добавление контроллеров с настройками JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Игнорирование циклических ссылок (если они есть)
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        // Конвертация дат в формат ISO
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .AddFluentValidationAutoValidation(); // Автоматическая валидация через FluentValidation

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

// ========== Авторизация (роли) ==========
builder.Services.AddAuthorization();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

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

// Аутентификация и авторизация (порядок важен)
app.UseAuthentication();
app.UseAuthorization();

// Маршрутизация контроллеров
app.MapControllers();

// ========================================================
// 4. Запуск приложения
// ========================================================
app.Run();
