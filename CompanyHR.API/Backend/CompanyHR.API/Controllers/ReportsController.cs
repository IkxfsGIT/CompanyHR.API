using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CompanyHR.API.Constants;
using CompanyHR.API.DTOs.Responses;
using CompanyHR.API.Services;
using OfficeOpenXml; // Для работы с Excel (требуется установка пакета EPPlus)

namespace CompanyHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin + "," + Roles.HR + "," + Roles.Manager)]
public class ReportsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStatisticsService _statisticsService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        IUnitOfWork unitOfWork,
        IStatisticsService statisticsService,
        ILogger<ReportsController> logger)
    {
        _unitOfWork = unitOfWork;
        _statisticsService = statisticsService;
        _logger = logger;
    }

    /// <summary>
    /// Получение отчёта по сотрудникам в формате JSON
    /// </summary>
    [HttpGet("employees")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeesReport(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? department,
        [FromQuery] string? position)
    {
        var employees = await _unitOfWork.Employees.GetAllAsync();
        
        var query = employees.AsQueryable();
        
        if (fromDate.HasValue)
            query = query.Where(e => e.HireDate >= fromDate.Value);
        
        if (toDate.HasValue)
            query = query.Where(e => e.HireDate <= toDate.Value);
        
        if (!string.IsNullOrEmpty(department))
            query = query.Where(e => e.Department == department);
        
        if (!string.IsNullOrEmpty(position))
            query = query.Where(e => e.Position == position);

        var report = new
        {
            GeneratedAt = DateTime.UtcNow,
            Filters = new
            {
                FromDate = fromDate?.ToString(ApplicationConstants.DateFormats.DefaultDateFormat),
                ToDate = toDate?.ToString(ApplicationConstants.DateFormats.DefaultDateFormat),
                Department = department,
                Position = position
            },
            Summary = new
            {
                TotalCount = query.Count(),
                ActiveCount = query.Count(e => e.IsActive),
                AverageSalary = query.Where(e => e.Salary.HasValue).Average(e => e.Salary ?? 0),
                MinHireDate = query.Min(e => e.HireDate),
                MaxHireDate = query.Max(e => e.HireDate)
            },
            Employees = query.Select(e => new
            {
                e.EmployeeId,
                FullName = $"{e.FirstName} {e.LastName}",
                e.Email,
                e.Phone,
                e.Position,
                e.Department,
                HireDate = e.HireDate.ToString(ApplicationConstants.DateFormats.DefaultDateFormat),
                e.Salary,
                e.IsActive,
                YearsInCompany = (DateTime.UtcNow.Year - e.HireDate.Year)
            }).OrderBy(e => e.HireDate)
        };

        return Ok(ApiResponse<object>.SuccessResponse(report));
    }

    /// <summary>
    /// Экспорт отчёта по сотрудникам в формате Excel
    /// </summary>
    [HttpGet("employees/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportEmployeesToExcel()
    {
        var employees = await _unitOfWork.Employees.GetAllAsync();
        
        // Настройка лицензии EPPlus (для некоммерческого использования)
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Сотрудники");

        // Заголовки
        worksheet.Cells[1, 1].Value = "ID";
        worksheet.Cells[1, 2].Value = "Фамилия";
        worksheet.Cells[1, 3].Value = "Имя";
        worksheet.Cells[1, 4].Value = "Email";
        worksheet.Cells[1, 5].Value = "Должность";
        worksheet.Cells[1, 6].Value = "Отдел";
        worksheet.Cells[1, 7].Value = "Дата найма";
        worksheet.Cells[1, 8].Value = "Зарплата";
        worksheet.Cells[1, 9].Value = "Статус";

        // Форматирование заголовков
        using (var range = worksheet.Cells[1, 1, 1, 9])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        // Данные
        var row = 2;
        foreach (var employee in employees.OrderBy(e => e.LastName))
        {
            worksheet.Cells[row, 1].Value = employee.EmployeeId;
            worksheet.Cells[row, 2].Value = employee.LastName;
            worksheet.Cells[row, 3].Value = employee.FirstName;
            worksheet.Cells[row, 4].Value = employee.Email;
            worksheet.Cells[row, 5].Value = employee.Position;
            worksheet.Cells[row, 6].Value = employee.Department;
            worksheet.Cells[row, 7].Value = employee.HireDate.ToString(ApplicationConstants.DateFormats.DefaultDateFormat);
            worksheet.Cells[row, 8].Value = employee.Salary;
            worksheet.Cells[row, 9].Value = employee.IsActive ? "Активен" : "Неактивен";
            
            // Форматирование зарплаты
            worksheet.Cells[row, 8].Style.Numberformat.Format = "#,##0.00";
            
            row++;
        }

        // Автоподбор ширины колонок
        worksheet.Cells.AutoFitColumns();

        var fileContents = package.GetAsByteArray();
        var fileName = $"employees_report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        
        return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    /// <summary>
    /// Получение отчёта по динамике найма
    /// </summary>
    [HttpGet("hiring-trends")]
    [ProducesResponseType(typeof(ApiResponse<HireTrendsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHiringTrends([FromQuery] int months = 12)
    {
        var trends = await _statisticsService.GetHireTrendsAsync(months);
        return Ok(ApiResponse<HireTrendsDto>.SuccessResponse(trends));
    }

    /// <summary>
    /// Получение отчёта по зарплатам
    /// </summary>
    [HttpGet("salary")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSalaryReport()
    {
        var employees = await _unitOfWork.Employees.FindAsync(e => e.Salary.HasValue);
        
        var report = new
        {
            Summary = new
            {
                TotalEmployees = employees.Count(),
                TotalSalaryBudget = employees.Sum(e => e.Salary ?? 0),
                AverageSalary = employees.Average(e => e.Salary ?? 0),
                MinSalary = employees.Min(e => e.Salary ?? 0),
                MaxSalary = employees.Max(e => e.Salary ?? 0)
            },
            ByDepartment = employees
                .Where(e => !string.IsNullOrEmpty(e.Department))
                .GroupBy(e => e.Department)
                .Select(g => new
                {
                    Department = g.Key,
                    EmployeeCount = g.Count(),
                    TotalSalary = g.Sum(e => e.Salary ?? 0),
                    AverageSalary = g.Average(e => e.Salary ?? 0)
                }),
            ByPosition = employees
                .Where(e => !string.IsNullOrEmpty(e.Position))
                .GroupBy(e => e.Position)
                .Select(g => new
                {
                    Position = g.Key,
                    EmployeeCount = g.Count(),
                    AverageSalary = g.Average(e => e.Salary ?? 0)
                })
        };

        return Ok(ApiResponse<object>.SuccessResponse(report));
    }
}
