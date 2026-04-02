[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    
    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllEmployees(
        [FromQuery] string? position,
        [FromQuery] DateTime? hireDateFrom,
        [FromQuery] DateTime? hireDateTo,
        [FromQuery] string? department,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var employees = await _employeeService.GetEmployeesAsync(
            position, hireDateFrom, hireDateTo, department, page, pageSize);
        
        return Ok(employees);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmployeeById(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null)
            return NotFound();
        
        return Ok(employee);
    }
    
    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> CreateEmployee(EmployeeCreateDto dto)
    {
        var employee = await _employeeService.CreateEmployeeAsync(dto);
        return CreatedAtAction(nameof(GetEmployeeById), 
            new { id = employee.EmployeeId }, employee);
    }
    
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> UpdateEmployee(int id, EmployeeUpdateDto dto)
    {
        var updated = await _employeeService.UpdateEmployeeAsync(id, dto);
        if (!updated)
            return NotFound();
        
        return NoContent();
    }
    
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var deleted = await _employeeService.DeleteEmployeeAsync(id);
        if (!deleted)
            return NotFound();
        
        return NoContent();
    }
}