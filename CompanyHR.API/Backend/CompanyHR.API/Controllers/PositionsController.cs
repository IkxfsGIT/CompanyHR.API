using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CompanyHR.API.Constants;
using CompanyHR.API.DTOs;
using CompanyHR.API.DTOs.Responses;
using CompanyHR.API.Filters;
using CompanyHR.API.Services;

namespace CompanyHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ValidateModel]
public class PositionsController : ControllerBase
{
    private readonly IPositionService _positionService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<PositionsController> _logger;

    public PositionsController(
        IPositionService positionService,
        ICacheService cacheService,
        ILogger<PositionsController> logger)
    {
        _positionService = positionService;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Получение списка всех должностей
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PositionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPositions()
    {
        var cachedPositions = await _cacheService.GetAsync<IEnumerable<PositionDto>>(ApplicationConstants.CacheKeys.AllPositions);
        if (cachedPositions != null)
        {
            _logger.LogInformation("Список должностей получен из кэша");
            return Ok(ApiResponse<IEnumerable<PositionDto>>.SuccessResponse(cachedPositions));
        }

        var positions = await _positionService.GetAllPositionsAsync();
        await _cacheService.SetAsync(ApplicationConstants.CacheKeys.AllPositions, positions, TimeSpan.FromHours(1));

        return Ok(ApiResponse<IEnumerable<PositionDto>>.SuccessResponse(positions));
    }

    /// <summary>
    /// Получение должности по идентификатору
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<PositionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPositionById(int id)
    {
        var position = await _positionService.GetPositionByIdAsync(id);
        if (position == null)
        {
            return NotFound(ApiResponse<PositionDto>.ErrorResponse($"Должность с ID {id} не найдена"));
        }

        return Ok(ApiResponse<PositionDto>.SuccessResponse(position));
    }

    /// <summary>
    /// Создание новой должности
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin + "," + Roles.HR)]
    [ProducesResponseType(typeof(ApiResponse<PositionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePosition([FromBody] CreatePositionDto dto)
    {
        try
        {
            var position = await _positionService.CreatePositionAsync(dto);
            
            // Инвалидация кэша
            await _cacheService.RemoveAsync(ApplicationConstants.CacheKeys.AllPositions);
            
            return CreatedAtAction(nameof(GetPositionById), new { id = position.PositionId }, 
                ApiResponse<PositionDto>.SuccessResponse(position, "Должность успешно создана"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании должности");
            return BadRequest(ApiResponse<PositionDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Обновление существующей должности
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.HR)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePosition(int id, [FromBody] UpdatePositionDto dto)
    {
        try
        {
            var updated = await _positionService.UpdatePositionAsync(id, dto);
            if (!updated)
            {
                return NotFound(ApiResponse<string>.ErrorResponse($"Должность с ID {id} не найдена"));
            }

            // Инвалидация кэша
            await _cacheService.RemoveAsync(ApplicationConstants.CacheKeys.AllPositions);
            
            return Ok(ApiResponse<string>.SuccessResponse(null, "Должность успешно обновлена"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении должности ID {Id}", id);
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Удаление должности
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeletePosition(int id)
    {
        try
        {
            var deleted = await _positionService.DeletePositionAsync(id);
            if (!deleted)
            {
                return NotFound(ApiResponse<string>.ErrorResponse($"Должность с ID {id} не найдена"));
            }

            // Инвалидация кэша
            await _cacheService.RemoveAsync(ApplicationConstants.CacheKeys.AllPositions);
            
            return Ok(ApiResponse<string>.SuccessResponse(null, "Должность успешно удалена"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении должности ID {Id}", id);
            return BadRequest(ApiResponse<string>.ErrorResponse("Ошибка при удалении должности"));
        }
    }

    /// <summary>
    /// Получение сотрудников по должности
    /// </summary>
    [HttpGet("{id}/employees")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployeesByPosition(int id)
    {
        var position = await _positionService.GetPositionByIdAsync(id);
        if (position == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Должность с ID {id} не найдена"));
        }

        // Здесь предполагается, что в модели Employee есть PositionId
        // Если нет - нужно адаптировать под вашу модель
        var employees = await _unitOfWork.Employees.FindAsync(e => e.PositionId == id && e.IsActive);
        
        var result = employees.Select(e => new
        {
            e.EmployeeId,
            FullName = $"{e.FirstName} {e.LastName}",
            e.Email,
            e.HireDate,
            e.Department
        });

        return Ok(ApiResponse<IEnumerable<object>>.SuccessResponse(result));
    }
}
