namespace CompanyHR.API.Constants;

/// <summary>
/// Общие константы приложения
/// </summary>
public static class ApplicationConstants
{
    // Название приложения
    public const string ApplicationName = "Company HR Management System";
    public const string ApplicationVersion = "1.0.0";
    
    // Политики авторизации
    public static class Policies
    {
        public const string RequireAdmin = "RequireAdmin";
        public const string RequireHR = "RequireHR";
        public const string RequireManager = "RequireManager";
        public const string RequireEmployee = "RequireEmployee";
        public const string RequireAdminOrHR = "RequireAdminOrHR";
    }
    
    // Ключи кэша
    public static class CacheKeys
    {
        public const string AllEmployees = "all_employees";
        public const string AllPositions = "all_positions";
        public const string AllDepartments = "all_departments";
        public const string EmployeeStatistics = "employee_statistics";
        public const string LastNotificationCheck = "last_notification_check";
        
        public static string GetEmployeeById(int id) => $"employee_{id}";
        public static string GetEmployeesByPosition(string position) => $"employees_position_{position}";
        public static string GetEmployeesByDepartment(string department) => $"employees_dept_{department}";
    }
    
    // Пути к файлам и шаблонам
    public static class Paths
    {
        public const string EmailTemplatesFolder = "Templates/Emails";
        public const string UploadsFolder = "Uploads";
        public const string EmployeePhotosFolder = "Uploads/Photos";
        
        public static string GetEmailTemplatePath(string templateName) => 
            $"{EmailTemplatesFolder}/{templateName}.html";
    }
    
    // Ограничения валидации
    public static class Validation
    {
        public const int MaxNameLength = 100;
        public const int MaxEmailLength = 150;
        public const int MaxPhoneLength = 20;
        public const int MinPasswordLength = 6;
        public const int MaxPasswordLength = 100;
        public const int MaxPositionLength = 100;
        public const int MaxDepartmentLength = 100;
        public const int MaxAddressLength = 200;
    }
    
    // Форматы дат
    public static class DateFormats
    {
        public const string DefaultDateFormat = "dd.MM.yyyy";
        public const string DefaultDateTimeFormat = "dd.MM.yyyy HH:mm:ss";
        public const string ApiDateFormat = "yyyy-MM-dd";
    }
    
    // Сообщения об ошибках
    public static class ErrorMessages
    {
        public const string Unauthorized = "Неавторизованный доступ";
        public const string Forbidden = "Доступ запрещён";
        public const string NotFound = "Ресурс не найден";
        public const string ValidationError = "Ошибка валидации";
        public const string ServerError = "Внутренняя ошибка сервера";
        
        public const string UserNotFound = "Пользователь не найден";
        public const string InvalidCredentials = "Неверный email или пароль";
        public const string EmailAlreadyExists = "Пользователь с таким email уже существует";
        public const string EmployeeNotFound = "Сотрудник не найден";
    }
    
    // Настройки пагинации по умолчанию
    public static class Pagination
    {
        public const int DefaultPage = 1;
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;
    }
    
    // Типы уведомлений
    public static class NotificationTypes
    {
        public const string Birthday = "Birthday";
        public const string WorkAnniversary = "WorkAnniversary";
        public const string NewEmployee = "NewEmployee";
        public const string EmployeeLeft = "EmployeeLeft";
        public const string System = "System";
    }
    
    // Расширения файлов
    public static class FileExtensions
    {
        public static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        public static readonly string[] AllowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
        
        public const string DefaultEmployeePhoto = "default-avatar.png";
    }
}
