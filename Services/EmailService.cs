using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace CompanyHR.API.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = false);
    Task SendEmailWithTemplateAsync(string to, string subject, string templateName, object model);
}

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;
    private readonly IWebHostEnvironment _env;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger, IWebHostEnvironment env)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
        _env = env;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            message.Body = new TextPart(isHtml ? TextFormat.Html : TextFormat.Plain)
            {
                Text = body
            };

            using var client = new SmtpClient();
            // Для разработки можно отключить проверку SSL
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email успешно отправлен на адрес {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке email на адрес {To}", to);
            throw;
        }
    }

    public async Task SendEmailWithTemplateAsync(string to, string subject, string templateName, object model)
    {
        // Пример использования шаблонов: можно использовать RazorEngine или простую замену
        // Упрощённо: читаем файл шаблона из папки Templates и заменяем плейсхолдеры
        var templatePath = Path.Combine(_env.ContentRootPath, "Templates", "Emails", $"{templateName}.html");
        if (!File.Exists(templatePath))
        {
            _logger.LogError("Шаблон {TemplateName} не найден по пути {Path}", templateName, templatePath);
            throw new FileNotFoundException($"Шаблон {templateName} не найден");
        }

        var templateContent = await File.ReadAllTextAsync(templatePath);
        // Простая замена {{PropertyName}} на значения из model
        var body = ReplacePlaceholders(templateContent, model);

        await SendEmailAsync(to, subject, body, isHtml: true);
    }

    private string ReplacePlaceholders(string template, object model)
    {
        // Очень простая реализация через Reflection
        var properties = model.GetType().GetProperties();
        foreach (var prop in properties)
        {
            var placeholder = $"{{{{{prop.Name}}}}}";
            var value = prop.GetValue(model)?.ToString() ?? "";
            template = template.Replace(placeholder, value);
        }
        return template;
    }
}

// Настройки для Email (добавить в appsettings.json)
public class EmailSettings
{
    public string SenderName { get; set; } = "Company HR";
    public string SenderEmail { get; set; } = "noreply@company.com";
    public string SmtpServer { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
}
