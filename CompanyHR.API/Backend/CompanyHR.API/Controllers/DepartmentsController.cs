using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CompanyHR.API.Constants;
using CompanyHR.API.DTOs.Responses;
using CompanyHR.API.Services;
using CompanyHR.API.Filters;

namespace CompanyHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ValidateModel]
public class DepartmentsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<DepartmentsController> _logger;

    public DepartmentsController(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<DepartmentsController> logger)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Получение списка всех отделов
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<string>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllDepartments()
    {
        // Попытка получения данных из кэша
        var cachedDepartments = await _cacheService.GetAsync<IEnumerable<string>>(ApplicationConstants.CacheKeys.AllDepartments);
        if (cachedDepartments != null)
        {
            _logger.LogInformation("Список отделов получен из кэша");
            return Ok(ApiResponse<IEnumerable<string>>.SuccessResponse(cachedDepartments));
        }

        // Получение списка уникальных отделов из базы данных
        var employees = await _unitOfWork.Employees.GetAllAsync();
        var departments = employees
            .Where(e => !string.IsNullOrEmpty(e.Department))
            .Select(e => e.Department!)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        // Сохранение в кэш
        await _cacheService.SetAsync(ApplicationConstants.CacheKeys.AllDepartments, departments, TimeSpan.FromHours(1));

        return Ok(ApiResponse<IEnumerable<string>>.SuccessResponse(departments));
    }

    /// <summary>
    /// Получение информации об отделе по названию
    /// </summary>
    [HttpGet("{departmentName}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDepartmentInfo(string departmentName)
    {
        var employees = await _unitOfWork.Employees.FindAsync(e => 
            e.Department != null && e.Department.Equals(departmentName, StringComparison.OrdinalIgnoreCase));

        if (!employees.Any())
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Отдел '{departmentName}' не найден"));
        }

        var employeeCount = employees.Count();
        var activeCount = employees.Count(e => e.IsActive);
        var averageSalary = employees.Where(e => e.Salary.HasValue).Average(e => e.Salary ?? 0);

        var result = new
        {
            DepartmentName = departmentName,
            EmployeeCount = employeeCount,
            ActiveEmployees = activeCount,
            AverageSalary = averageSalary,
            Employees = employees.Select(e => new
            {
                e.EmployeeId,
                FullName = $"{e.FirstName} {e.LastName}",
                e.Position,
                e.HireDate,
                e.IsActive
            })
        };

        return Ok(ApiResponse<object>.SuccessResponse(result));
    }

    /// <summary>
    /// Получение статистики по отделам
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = Roles.Admin + "," + Roles.HR)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartmentStatistics()
    {
        var employees = await _unitOfWork.Employees.GetAllAsync();
        
        var statistics = employees
            .Where(e => !string.IsNullOrEmpty(e.Department))
            .GroupBy(e => e.Department)
            .Select(g => new
            {
                Department = g.Key,
                EmployeeCount = g.Count(),
                ActiveCount = g.Count(e => e.IsActive),
                AverageSalary = g.Where(e => e.Salary.HasValue).Average(e => e.Salary ?? 0),
                MinSalary = g.Where(e => e.Salary.HasValue).Min(e => e.Salary ?? 0),
                MaxSalary = g.Where(e => e.Salary.HasValue).Max(e => e.Salary ?? 0)
            })
            .OrderByDescending(x => x.EmployeeCount)
            .ToList();

        return Ok(ApiResponse<object>.SuccessResponse(statistics));
    }
}
