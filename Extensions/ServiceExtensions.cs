using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CompanyHR.API.Data;
using CompanyHR.API.Repositories;
using CompanyHR.API.Repositories.Interfaces;
using CompanyHR.API.Services;
using CompanyHR.API.BackgroundServices;
using CompanyHR.API.Helpers;
using StackExchange.Redis;

namespace CompanyHR.API.Extensions;

/// <summary>
/// Класс с методами расширения для регистрации сервисов в DI-контейнере
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Добавление сервисов базы данных
    /// </summary>
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            // Использование InMemory для разработки
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("CompanyHR"));
        }
        else
        {
            // Использование PostgreSQL
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        return services;
    }

    /// <summary>
    /// Добавление репозиториев и Unit of Work
    /// </summary>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        
        return services;
    }

    /// <summary>
    /// Добавление сервисов приложения
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Бизнес-сервисы
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IPositionService, PositionService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();
        
        // Вспомогательные сервисы
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<JwtHelper>();
        
        return services;
    }

    /// <summary>
    /// Добавление фоновых служб
    /// </summary>
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<EmployeeNotificationService>();
        
        return services;
    }

    /// <summary>
    /// Добавление кэширования
    /// </summary>
    public static IServiceCollection AddCachingServices(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis");
        
        if (!string.IsNullOrEmpty(redisConnection))
        {
            // Использование Redis
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "CompanyHR_";
            });
            
            services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(redisConnection));
        }
        else
        {
            // Использование In-Memory кэша для разработки
            services.AddDistributedMemoryCache();
        }

        return services;
    }

    /// <summary>
    /// Добавление CORS политик
    /// </summary>
    public static IServiceCollection AddCorsPolicies(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("Development", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });

            options.AddPolicy("Production", policy =>
            {
                policy.WithOrigins(
                        "http://localhost:8080",
                        "http://localhost:3000",
                        "https://company-hr-frontend.vercel.app")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });

            options.AddPolicy("FlutterApp", policy =>
            {
                policy.WithOrigins(
                        "http://localhost:8080",
                        "http://localhost:5000",
                        "http://127.0.0.1:8080")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        return services;
    }

    /// <summary>
    /// Добавление настроек аутентификации
    /// </summary>
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<EmailSettings>(configuration.GetSection("Email"));

        return services;
    }

    /// <summary>
    /// Добавление всех сервисов одной командой
    /// </summary>
    public static IServiceCollection AddAllServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabaseServices(configuration);
        services.AddRepositories();
        services.AddApplicationServices();
        services.AddBackgroundServices();
        services.AddCachingServices(configuration);
        services.AddCorsPolicies();
        services.AddAuthenticationServices(configuration);
        
        return services;
    }
}
