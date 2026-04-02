using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CompanyHR.API.Constants;
using CompanyHR.API.DTOs.Responses;
using CompanyHR.API.Services;

namespace CompanyHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin + "," + Roles.HR + "," + Roles.Manager)]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _statisticsService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<StatisticsController> _logger;

    public StatisticsController(
        IStatisticsService statisticsService,
        ICacheService cacheService,
        ILogger<StatisticsController> logger)
    {
        _statisticsService = statisticsService;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Получение общей статистики по сотрудникам
    /// </summary>
    [HttpGet("employees")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeStatistics()
    {
        var cachedStats = await _cacheService.GetAsync<EmployeeStatisticsDto>(ApplicationConstants.CacheKeys.EmployeeStatistics);
        if (cachedStats != null)
        {
            return Ok(ApiResponse<EmployeeStatisticsDto>.SuccessResponse(cachedStats));
        }

        var statistics = await _statisticsService.GetEmployeeStatisticsAsync();
        await _cacheService.SetAsync(ApplicationConstants.CacheKeys.EmployeeStatistics, statistics, TimeSpan.FromMinutes(30));

        return Ok(ApiResponse<EmployeeStatisticsDto>.SuccessResponse(statistics));
    }

    /// <summary>
    /// Получение статистики по отделам
    /// </summary>
    [HttpGet("departments")]
    [ProducesResponseType(typeof(ApiResponse<DepartmentStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartmentStatistics()
    {
        var statistics = await _statisticsService.GetDepartmentStatisticsAsync();
        return Ok(ApiResponse<DepartmentStatisticsDto>.SuccessResponse(statistics));
    }

    /// <summary>
    /// Получение трендов найма
    /// </summary>
    [HttpGet("hiring-trends")]
    [ProducesResponseType(typeof(ApiResponse<HireTrendsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHiringTrends([FromQuery] int months = 12)
    {
        var trends = await _statisticsService.GetHireTrendsAsync(months);
        return Ok(ApiResponse<HireTrendsDto>.SuccessResponse(trends));
    }

    /// <summary>
    /// Получение статистики по должностям
    /// </summary>
    [HttpGet("positions")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPositionStatistics()
    {
        var employees = await _unitOfWork.Employees.GetAllAsync();
        
        var statistics = employees
            .Where(e => !string.IsNullOrEmpty(e.Position))
            .GroupBy(e => e.Position)
            .Select(g => new
            {
                Position = g.Key,
                Count = g.Count(),
                ActiveCount = g.Count(e => e.IsActive),
                AverageSalary = g.Where(e => e.Salary.HasValue).Average(e => e.Salary ?? 0),
                NewHiresLastMonth = g.Count(e => e.HireDate >= DateTime.UtcNow.AddMonths(-1))
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        return Ok(ApiResponse<object>.SuccessResponse(statistics));
    }

    /// <summary>
    /// Получение статистики по стажу работы
    /// </summary>
    [HttpGet("tenure")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTenureStatistics()
    {
        var employees = await _unitOfWork.Employees.FindAsync(e => e.IsActive);
        var now = DateTime.UtcNow;

        var tenureGroups = new
        {
            LessThanOneYear = employees.Count(e => (now - e.HireDate).TotalDays < 365),
            OneToThreeYears = employees.Count(e => 
                (now - e.HireDate).TotalDays >= 365 && 
                (now - e.HireDate).TotalDays < 1095),
            ThreeToFiveYears = employees.Count(e => 
                (now - e.HireDate).TotalDays >= 1095 && 
                (now - e.HireDate).TotalDays < 1825),
            FiveToTenYears = employees.Count(e => 
                (now - e.HireDate).TotalDays >= 1825 && 
                (now - e.HireDate).TotalDays < 3650),
            MoreThanTenYears = employees.Count(e => (now - e.HireDate).TotalDays >= 3650)
        };

        return Ok(ApiResponse<object>.SuccessResponse(tenureGroups));
    }

    /// <summary>
    /// Получение ежемесячной статистики за указанный год
    /// </summary>
    [HttpGet("monthly/{year}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMonthlyStatistics(int year)
    {
        var employees = await _unitOfWork.Employees.GetAllAsync();
        
        var monthlyStats = Enumerable.Range(1, 12)
            .Select(month => new
            {
                Month = month,
                MonthName = new DateTime(year, month, 1).ToString("MMMM", new System.Globalization.CultureInfo("ru-RU")),
                Hired = employees.Count(e => e.HireDate.Year == year && e.HireDate.Month == month),
                Terminated = employees.Count(e => e.HireDate.Year == year && e.HireDate.Month == month && !e.IsActive)
            })
            .ToList();

        var result = new
        {
            Year = year,
            TotalHired = monthlyStats.Sum(m => m.Hired),
            TotalTerminated = monthlyStats.Sum(m => m.Terminated),
            MonthlyData = monthlyStats
        };

        return Ok(ApiResponse<object>.SuccessResponse(result));
    }

    /// <summary>
    /// Получение статистики по дням рождения (для виджета)
    /// </summary>
    [HttpGet("birthdays")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUpcomingBirthdays([FromQuery] int days = 30)
    {
        var employees = await _unitOfWork.Employees.FindAsync(e => e.IsActive && e.BirthDate.HasValue);
        var today = DateTime.Today;
        var endDate = today.AddDays(days);

        var upcomingBirthdays = employees
            .Select(e => new
            {
                e.EmployeeId,
                FullName = $"{e.FirstName} {e.LastName}",
                e.BirthDate,
                NextBirthday = GetNextBirthday(e.BirthDate.Value, today),
                e.Department,
                e.Position
            })
            .Where(x => x.NextBirthday >= today && x.NextBirthday <= endDate)
            .OrderBy(x => x.NextBirthday)
            .ToList();

        return Ok(ApiResponse<object>.SuccessResponse(upcomingBirthdays));
    }

    private DateTime GetNextBirthday(DateTime birthDate, DateTime fromDate)
    {
        var nextBirthday = new DateTime(fromDate.Year, birthDate.Month, birthDate.Day);
        if (nextBirthday < fromDate)
        {
            nextBirthday = nextBirthday.AddYears(1);
        }
        return nextBirthday;
    }
}
