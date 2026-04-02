namespace CompanyHR.API.Constants;

/// <summary>
/// Константы ролей пользователей в системе
/// </summary>
public static class Roles
{
    // Основные роли
    public const string Admin = "Admin";
    public const string HR = "HR";
    public const string Manager = "Manager";
    public const string Employee = "Employee";
    
    // Составные роли для удобства
    public static class Sets
    {
        public static readonly string[] All = { Admin, HR, Manager, Employee };
        public static readonly string[] AdminAndHR = { Admin, HR };
        public static readonly string[] Management = { Admin, HR, Manager };
        public static readonly string[] NonEmployee = { Admin, HR, Manager };
    }
    
    // Описания ролей
    public static class Descriptions
    {
        public const string Admin = "Администратор системы (полный доступ)";
        public const string HR = "Сотрудник отдела кадров (управление сотрудниками)";
        public const string Manager = "Руководитель (доступ к своей команде)";
        public const string Employee = "Сотрудник (доступ только к своим данным)";
    }
    
    /// <summary>
    /// Проверка, является ли роль административной
    /// </summary>
    public static bool IsAdminRole(string role) => role == Admin || role == HR;
    
    /// <summary>
    /// Проверка, имеет ли роль доступ к управлению сотрудниками
    /// </summary>
    public static bool CanManageEmployees(string role) => role == Admin || role == HR;
    
    /// <summary>
    /// Проверка, имеет ли роль доступ к отчётам
    /// </summary>
    public static bool CanViewReports(string role) => role == Admin || role == HR || role == Manager;
    
    /// <summary>
    /// Получить нормализованное название роли
    /// </summary>
    public static string NormalizeRole(string role)
    {
        return role?.Trim() switch
        {
            string r when r.Equals(Admin, StringComparison.OrdinalIgnoreCase) => Admin,
            string r when r.Equals(HR, StringComparison.OrdinalIgnoreCase) => HR,
            string r when r.Equals(Manager, StringComparison.OrdinalIgnoreCase) => Manager,
            string r when r.Equals(Employee, StringComparison.OrdinalIgnoreCase) => Employee,
            _ => Employee // По умолчанию
        };
    }
    
    /// <summary>
    /// Проверка существования роли
    /// </summary>
    public static bool IsValidRole(string role)
    {
        return Sets.All.Contains(role, StringComparer.OrdinalIgnoreCase);
    }
    
    /// <summary>
    /// Получить все доступные роли
    /// </summary>
    public static IEnumerable<string> GetAllRoles() => Sets.All;
    
    /// <summary>
    /// Получить роль с описанием
    /// </summary>
    public static Dictionary<string, string> GetRolesWithDescriptions() => new()
    {
        [Admin] = Descriptions.Admin,
        [HR] = Descriptions.HR,
        [Manager] = Descriptions.Manager,
        [Employee] = Descriptions.Employee
    };
}

/// <summary>
/// Атрибут для указания допустимых ролей в методах контроллеров
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class RequireRoleAttribute : Attribute
{
    public string[] Roles { get; }
    
    public RequireRoleAttribute(params string[] roles)
    {
        Roles = roles;
    }
}
