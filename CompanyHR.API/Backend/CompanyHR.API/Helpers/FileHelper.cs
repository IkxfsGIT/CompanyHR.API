using Microsoft.AspNetCore.Http;

namespace CompanyHR.API.Helpers;

/// <summary>
/// Вспомогательный класс для операций с файлами
/// </summary>
public static class FileHelper
{
    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
    private static readonly string[] AllowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    /// <summary>
    /// Сохранение файла на диск
    /// </summary>
    public static async Task<string> SaveFileAsync(IFormFile file, string folderPath, string? fileName = null)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Файл не выбран или пуст");

        // Создание папки, если не существует
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        // Генерация имени файла
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var safeFileName = fileName ?? $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(folderPath, safeFileName);

        // Сохранение
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return safeFileName;
    }

    /// <summary>
    /// Удаление файла
    /// </summary>
    public static bool DeleteFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Проверка, является ли файл изображением
    /// </summary>
    public static bool IsImageFile(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedImageExtensions.Contains(extension);
    }

    /// <summary>
    /// Проверка, является ли файл документом
    /// </summary>
    public static bool IsDocumentFile(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedDocumentExtensions.Contains(extension);
    }

    /// <summary>
    /// Проверка размера файла
    /// </summary>
    public static bool IsFileSizeValid(long fileSize, long maxSize = MaxFileSize)
    {
        return fileSize <= maxSize;
    }

    /// <summary>
    /// Получение безопасного имени файла (удаление недопустимых символов)
    /// </summary>
    public static string GetSafeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>
    /// Получение MIME-типа по расширению
    /// </summary>
    public static string GetMimeType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }

    /// <summary>
    /// Чтение файла как массива байт
    /// </summary>
    public static async Task<byte[]> ReadAllBytesAsync(string filePath)
    {
        return await File.ReadAllBytesAsync(filePath);
    }

    /// <summary>
    /// Получение информации о файле
    /// </summary>
    public static FileInfo GetFileInfo(string filePath)
    {
        return new FileInfo(filePath);
    }
}
