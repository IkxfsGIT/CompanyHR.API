using Microsoft.EntityFrameworkCore;
using CompanyHR.API.Data;
using CompanyHR.API.Services;

namespace CompanyHR.API.BackgroundServices;

/// <summary>
/// Фоновая служба для отправки уведомлений сотрудникам (дни рождения, годовщины и т.д.)
/// </summary>
public class EmployeeNotificationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmployeeNotificationService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // Проверка раз в сутки
    private readonly TimeSpan _notificationTime = new TimeSpan(9, 0, 0); // Отправка в 9:00

    public EmployeeNotificationService(
        IServiceProvider serviceProvider,
        ILogger<EmployeeNotificationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Служба уведомлений сотрудников запущена");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Вычисление времени до следующего запуска
                var nextRun = CalculateNextRunTime();
                var delay = nextRun - DateTime.Now;
                
                if (delay > TimeSpan.Zero)
                {
                    _logger.LogInformation("Следующая проверка уведомлений в {NextRun}", nextRun);
                    await Task.Delay(delay, stoppingToken);
                }

                // Выполняем проверку и отправку уведомлений
                await CheckAndSendNotifications(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Служба уведомлений остановлена");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при выполнении службы уведомлений");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Пауза при ошибке
            }
        }
    }

    private DateTime CalculateNextRunTime()
    {
        var now = DateTime.Now;
        var nextRun = now.Date.Add(_notificationTime); // Сегодня в 9:00
        
        if (nextRun <= now)
        {
            nextRun = nextRun.AddDays(1); // Завтра в 9:00
        }
        
        return nextRun;
    }

    private async Task CheckAndSendNotifications(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

        // Получаем текущую дату
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        // 1. Проверка дней рождения сегодня
        var todayBirthdays = await context.Employees
            .Include(e => e.User)
            .Where(e => e.IsActive && 
                       e.BirthDate.HasValue && 
                       e.BirthDate.Value.Month == today.Month && 
                       e.BirthDate.Value.Day == today.Day)
            .ToListAsync(cancellationToken);

        foreach (var employee in todayBirthdays)
        {
            try
            {
                await SendBirthdayNotification(employee, emailService);
                
                // Логируем отправку
                _logger.LogInformation("Отправлено поздравление с днём рождения сотруднику {FullName}", 
                    $"{employee.FirstName} {employee.LastName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке поздравления сотруднику ID {EmployeeId}", 
                    employee.EmployeeId);
            }
        }

        // 2. Проверка дней рождения завтра (для планирования)
        var tomorrowBirthdays = await context.Employees
            .Where(e => e.IsActive && 
                       e.BirthDate.HasValue && 
                       e.BirthDate.Value.Month == tomorrow.Month && 
                       e.BirthDate.Value.Day == tomorrow.Day)
            .CountAsync(cancellationToken);

        if (tomorrowBirthdays > 0)
        {
            // Можно отправить напоминание HR-отделу
            await SendBirthdayReminderToHR(tomorrowBirthdays, emailService);
        }

        // 3. Проверка годовщин работы
        var workAnniversaries = await context.Employees
            .Include(e => e.User)
            .Where(e => e.IsActive && 
                       e.HireDate.Month == today.Month && 
                       e.HireDate.Day == today.Day)
            .ToListAsync(cancellationToken);

        foreach (var employee in workAnniversaries)
        {
            var yearsWorked = today.Year - employee.HireDate.Year;
            if (yearsWorked > 0 && yearsWorked % 5 == 0) // Каждые 5 лет
            {
                await SendWorkAnniversaryNotification(employee, yearsWorked, emailService);
            }
        }

        // 4. Сохранение времени последней проверки в кэш
        await cacheService.SetAsync("last_notification_check", DateTime.UtcNow);
    }

    private async Task SendBirthdayNotification(Employee employee, IEmailService emailService)
    {
        var subject = "С днём рождения!";
        var body = $@"
            <h2>Уважаемый(ая) {employee.FirstName} {employee.LastName}!</h2>
            <p>Коллектив компании поздравляет Вас с днём рождения!</p>
            <p>Желаем Вам здоровья, счастья, успехов в работе и исполнения всех желаний!</p>
            <p>С уважением, HR отдел</p>";

        await emailService.SendEmailAsync(employee.Email, subject, body, isHtml: true);
    }

    private async Task SendBirthdayReminderToHR(int count, IEmailService emailService)
    {
        // Здесь можно получить email HR-отдела из конфигурации
        var hrEmail = "hr@company.com"; // В идеале - из настроек
        
        var subject = "Напоминание о днях рождения";
        var body = $@"
            <h2>Напоминание о предстоящих днях рождения</h2>
            <p>Завтра день рождения отмечают {count} сотрудников.</p>
            <p>Не забудьте поздравить!</p>";

        await emailService.SendEmailAsync(hrEmail, subject, body, isHtml: true);
    }

    private async Task SendWorkAnniversaryNotification(Employee employee, int years, IEmailService emailService)
    {
        var subject = $"{years} лет в компании!";
        var body = $@"
            <h2>Уважаемый(ая) {employee.FirstName} {employee.LastName}!</h2>
            <p>Сегодня исполняется {years} лет, как Вы работаете в нашей компании!</p>
            <p>Благодарим Вас за вклад в развитие компании и желаем дальнейших успехов!</p>
            <p>С уважением, HR отдел</p>";

        await emailService.SendEmailAsync(employee.Email, subject, body, isHtml: true);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Служба уведомлений сотрудников останавливается...");
        await base.StopAsync(cancellationToken);
    }
}
