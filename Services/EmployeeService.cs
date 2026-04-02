public class EmployeeService : IEmployeeService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    
    public EmployeeService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public async Task<PagedResult<EmployeeDto>> GetEmployeesAsync(
        string? position,
        DateTime? hireDateFrom,
        DateTime? hireDateTo,
        string? department,
        int page,
        int pageSize)
    {
        var query = _context.Employees
            .Include(e => e.Position)
            .Where(e => e.IsActive)
            .AsQueryable();
        
        // Фильтрация по должности
        if (!string.IsNullOrEmpty(position))
        {
            query = query.Where(e => e.Position.PositionName.Contains(position));
        }
        
        // Фильтрация по дате найма
        if (hireDateFrom.HasValue)
        {
            query = query.Where(e => e.HireDate >= hireDateFrom.Value);
        }
        
        if (hireDateTo.HasValue)
        {
            query = query.Where(e => e.HireDate <= hireDateTo.Value);
        }
        
        // Фильтрация по отделу
        if (!string.IsNullOrEmpty(department))
        {
            query = query.Where(e => e.Department.Contains(department));
        }
        
        // Пагинация
        var totalCount = await query.CountAsync();
        var employees = await query
            .OrderByDescending(e => e.HireDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        var employeeDtos = _mapper.Map<List<EmployeeDto>>(employees);
        
        return new PagedResult<EmployeeDto>
        {
            Items = employeeDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
}