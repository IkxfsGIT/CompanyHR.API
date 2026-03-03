using Microsoft.OpenApi.Models;
using CompanyHR.API.Constants;

namespace CompanyHR.API.Extensions;

/// <summary>
/// Класс с методами расширения для настройки Swagger
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Добавление базовой конфигурации Swagger
    /// </summary>
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = ApplicationConstants.ApplicationName,
                Version = ApplicationConstants.ApplicationVersion,
                Description = "API для управления сотрудниками компании с поддержкой полного цикла учёта персонала",
                Contact = new OpenApiContact
                {
                    Name = "HR Department",
                    Email = "hr@company.com",
                    Url = new Uri("https://company.com/hr")
                },
                License = new OpenApiLicense
                {
                    Name = "Internal Use Only",
                    Url = new Uri("https://company.com/license")
                }
            });

            // Добавление поддержки JWT
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

            // Включение XML-комментариев для документации
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            // Добавление поддержки аннотаций
            c.EnableAnnotations();
        });

        return services;
    }

    /// <summary>
    /// Добавление Swagger с поддержкой версионирования
    /// </summary>
    public static IServiceCollection AddSwaggerWithVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        });

        services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = $"{ApplicationConstants.ApplicationName} v1",
                Version = "v1",
                Description = "Первая версия API"
            });

            c.SwaggerDoc("v2", new OpenApiInfo
            {
                Title = $"{ApplicationConstants.ApplicationName} v2",
                Version = "v2",
                Description = "Вторая версия API (расширенная)"
            });

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

            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    /// <summary>
    /// Использование Swagger в приложении
    /// </summary>
    public static IApplicationBuilder UseSwaggerWithUI(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{ApplicationConstants.ApplicationName} v1");
            c.RoutePrefix = "swagger";
            c.DocumentTitle = $"{ApplicationConstants.ApplicationName} - Документация API";
            c.DefaultModelsExpandDepth(2);
            c.DefaultModelExpandDepth(2);
            c.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Model);
            c.DisplayRequestDuration();
            c.EnableDeepLinking();
            c.EnableFilter();
            c.ShowExtensions();

            if (env.IsDevelopment())
            {
                c.EnableValidator(null); // Отключение валидатора в разработке
            }
        });

        return app;
    }
}
