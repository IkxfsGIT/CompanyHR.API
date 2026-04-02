using Microsoft.EntityFrameworkCore;
using CompanyHR.API.Data;
using CompanyHR.API.DTOs;
using CompanyHR.API.Models;
using AutoMapper;

namespace CompanyHR.API.Services;

public interface IPositionService
{
    Task<IEnumerable<PositionDto>> GetAllPositionsAsync();
    Task<PositionDto?> GetPositionByIdAsync(int id);
    Task<PositionDto> CreatePositionAsync(CreatePositionDto dto);
    Task<bool> UpdatePositionAsync(int id, UpdatePositionDto dto);
    Task<bool> DeletePositionAsync(int id);
    Task<bool> PositionExistsAsync(int id);
}

public class PositionService : IPositionService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<PositionService> _logger;

    public PositionService(ApplicationDbContext context, IMapper mapper, ILogger<PositionService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<PositionDto>> GetAllPositionsAsync()
    {
        var positions = await _context.Positions.ToListAsync();
        return _mapper.Map<IEnumerable<PositionDto>>(positions);
    }

    public async Task<PositionDto?> GetPositionByIdAsync(int id)
    {
        var position = await _context.Positions.FindAsync(id);
        return position == null ? null : _mapper.Map<PositionDto>(position);
    }

    public async Task<PositionDto> CreatePositionAsync(CreatePositionDto dto)
    {
        var position = _mapper.Map<Position>(dto);
        position.CreatedAt = DateTime.UtcNow;

        _context.Positions.Add(position);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Создана новая должность: {PositionName} (ID: {Id})", position.PositionName, position.PositionId);
        return _mapper.Map<PositionDto>(position);
    }

    public async Task<bool> UpdatePositionAsync(int id, UpdatePositionDto dto)
    {
        var position = await _context.Positions.FindAsync(id);
        if (position == null)
            return false;

        // Обновляем только переданные поля
        if (!string.IsNullOrWhiteSpace(dto.PositionName))
            position.PositionName = dto.PositionName;
        
        if (!string.IsNullOrWhiteSpace(dto.Description))
            position.Description = dto.Description;
        
        if (dto.Level.HasValue)
            position.Level = dto.Level.Value;

        position.UpdatedAt = DateTime.UtcNow;

        _context.Positions.Update(position);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Обновлена должность ID {Id}", id);
        return true;
    }

    public async Task<bool> DeletePositionAsync(int id)
    {
        var position = await _context.Positions.FindAsync(id);
        if (position == null)
            return false;

        // Проверка, есть ли сотрудники с этой должностью
        var hasEmployees = await _context.Employees.AnyAsync(e => e.PositionId == id);
        if (hasEmployees)
        {
            _logger.LogWarning("Невозможно удалить должность ID {Id}: есть связанные сотрудники", id);
            throw new InvalidOperationException("Нельзя удалить должность, к которой привязаны сотрудники");
        }

        _context.Positions.Remove(position);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Удалена должность ID {Id}", id);
        return true;
    }

    public async Task<bool> PositionExistsAsync(int id)
    {
        return await _context.Positions.AnyAsync(p => p.PositionId == id);
    }
}

// DTO для должностей (если ещё не созданы)
public class PositionDto
{
    public int PositionId { get; set; }
    public string PositionName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Level { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreatePositionDto
{
    public string PositionName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Level { get; set; } = 1;
}

public class UpdatePositionDto
{
    public string? PositionName { get; set; }
    public string? Description { get; set; }
    public int? Level { get; set; }
}
