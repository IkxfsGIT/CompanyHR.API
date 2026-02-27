using Microsoft.EntityFrameworkCore;
using CompanyHR.API.Data;
using CompanyHR.API.DTOs.Responses;

namespace CompanyHR.API.Services;

public interface IStatisticsService
{
    Task<EmployeeStatisticsDto> GetEmployeeStatisticsAsync();
    Task<DepartmentStatisticsDto> GetDepartmentStatisticsAsync();
    Task<HireTrendsDto> GetHireTrendsAsync(int months = 12);
}

public class StatisticsService : IStatisticsService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StatisticsService> _logger;

    public StatisticsService(ApplicationDbContext context, ILogger<StatisticsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<EmployeeStatisticsDto> GetEmployeeStatisticsAsync()
    {
        try
        {
            var totalEmployees = await _context.Employees.CountAsync();
            var activeEmployees = await _context.Employees.CountAsync(e => e.IsActive);
            var avgSalary = await _context.Employees.Where(e => e.Salary.HasValue).AverageAsync(e => e.Salary ?? 0);
            var newThisMonth = await _context.Employees
                .CountAsync(e => e.HireDate.Month == DateTime.UtcNow.Month && e.HireDate.Year == DateTime.UtcNow.Year);
            var employeesByPosition = await _context.Employees
                .Where(e => e.IsActive)
                .GroupBy(e => e.Position)
                .Select(g => new { Position = g.Key, Count = g.Count() })
                .ToDictionaryAsync(k => k.Position ?? "Не указана", v => v.Count);

            return new EmployeeStatisticsDto
            {
                TotalEmployees = totalEmployees,
                ActiveEmployees = activeEmployees,
                AverageSalary = avgSalary,
                NewEmployeesThisMonth = newThisMonth,
                EmployeesByPosition = employeesByPosition
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении статистики сотрудников");
            throw;
        }
    }

    public async Task<DepartmentStatisticsDto> GetDepartmentStatisticsAsync()
    {
        try
        {
            var employeesByDept = await _context.Employees
                .Where(e => e.IsActive && e.Department != null)
                .GroupBy(e => e.Department)
                .Select(g => new { Department = g.Key, Count = g.Count() })
                .ToDictionaryAsync(k => k.Department!, v => v.Count);

            var avgSalaryByDept = await _context.Employees
                .Where(e => e.IsActive && e.Department != null && e.Salary.HasValue)
                .GroupBy(e => e.Department)
                .Select(g => new { Department = g.Key, AvgSalary = g.Average(e => e.Salary ?? 0) })
                .ToDictionaryAsync(k => k.Department!, v => v.AvgSalary);

            var newHiresByDept = await _context.Employees
                .Where(e => e.HireDate.Month == DateTime.UtcNow.Month && e.HireDate.Year == DateTime.UtcNow.Year && e.Department != null)
                .GroupBy(e => e.Department)
                .Select(g => new { Department = g.Key, Count = g.Count() })
                .ToDictionaryAsync(k => k.Department!, v => v.Count);

            return new DepartmentStatisticsDto
            {
                EmployeesByDepartment = employeesByDept,
                AverageSalaryByDepartment = avgSalaryByDept,
                NewHiresByDepartment = newHiresByDept
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении статистики по отделам");
            throw;
        }
    }

    public async Task<HireTrendsDto> GetHireTrendsAsync(int months = 12)
    {
        try
        {
            var startDate = DateTime.UtcNow.AddMonths(-months);
            var hires = await _context.Employees
                .Where(e => e.HireDate >= startDate)
                .GroupBy(e => new { e.HireDate.Year, e.HireDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            var monthlyHires = hires.ToDictionary(
                x => $"{x.Year}-{x.Month:D2}",
                x => x.Count
            );

            var yearlyHires = await _context.Employees
                .GroupBy(e => e.HireDate.Year)
                .Select(g => new { Year = g.Key, Count = g.Count() })
                .OrderBy(x => x.Year)
                .ToDictionaryAsync(k => k.Year, v => v.Count);

            var topPositions = await _context.Employees
                .Where(e => e.Position != null)
                .GroupBy(e => e.Position)
                .Select(g => new { Position = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .Select(x => $"{x.Position} ({x.Count})")
                .ToListAsync();

            return new HireTrendsDto
            {
                MonthlyHires = monthlyHires,
                YearlyHires = yearlyHires,
                TopPositions = topPositions
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении трендов найма");
            throw;
        }
    }
}

// DTO для статистики (если ещё не созданы)
public class EmployeeStatisticsDto
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public decimal AverageSalary { get; set; }
    public int NewEmployeesThisMonth { get; set; }
    public Dictionary<string, int> EmployeesByPosition { get; set; } = new();
}

public class DepartmentStatisticsDto
{
    public Dictionary<string, int> EmployeesByDepartment { get; set; } = new();
    public Dictionary<string, decimal> AverageSalaryByDepartment { get; set; } = new();
    public Dictionary<string, int> NewHiresByDepartment { get; set; } = new();
}

public class HireTrendsDto
{
    public Dictionary<string, int> MonthlyHires { get; set; } = new();
    public Dictionary<int, int> YearlyHires { get; set; } = new();
    public List<string> TopPositions { get; set; } = new();
}
