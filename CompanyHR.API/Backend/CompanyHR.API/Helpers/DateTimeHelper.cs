namespace CompanyHR.API.Helpers;

/// <summary>
/// Вспомогательный класс для операций с датами и временем
/// </summary>
public static class DateTimeHelper
{
    /// <summary>
    /// Получение текущей даты и времени в UTC
    /// </summary>
    public static DateTime NowUtc => DateTime.UtcNow;

    /// <summary>
    /// Получение текущей даты (без времени) в UTC
    /// </summary>
    public static DateTime TodayUtc => DateTime.UtcNow.Date;

    /// <summary>
    /// Преобразование локальной даты в UTC
    /// </summary>
    public static DateTime ToUtc(DateTime localDateTime, TimeZoneInfo? sourceTimeZone = null)
    {
        sourceTimeZone ??= TimeZoneInfo.Local;
        return TimeZoneInfo.ConvertTimeToUtc(localDateTime, sourceTimeZone);
    }

    /// <summary>
    /// Преобразование UTC даты в локальную для указанного часового пояса
    /// </summary>
    public static DateTime FromUtc(DateTime utcDateTime, TimeZoneInfo targetTimeZone)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, targetTimeZone);
    }

    /// <summary>
    /// Получение начала дня для указанной даты
    /// </summary>
    public static DateTime StartOfDay(DateTime date)
    {
        return date.Date;
    }

    /// <summary>
    /// Получение конца дня для указанной даты
    /// </summary>
    public static DateTime EndOfDay(DateTime date)
    {
        return date.Date.AddDays(1).AddTicks(-1);
    }

    /// <summary>
    /// Получение начала месяца для указанной даты
    /// </summary>
    public static DateTime StartOfMonth(DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    /// <summary>
    /// Получение конца месяца для указанной даты
    /// </summary>
    public static DateTime EndOfMonth(DateTime date)
    {
        return StartOfMonth(date).AddMonths(1).AddTicks(-1);
    }

    /// <summary>
    /// Получение начала года для указанной даты
    /// </summary>
    public static DateTime StartOfYear(int year)
    {
        return new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    /// <summary>
    /// Получение конца года
    /// </summary>
    public static DateTime EndOfYear(int year)
    {
        return new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);
    }

    /// <summary>
    /// Форматирование даты в строку согласно указанному формату
    /// </summary>
    public static string FormatDate(DateTime date, string format = "yyyy-MM-dd")
    {
        return date.ToString(format);
    }

    /// <summary>
    /// Форматирование даты и времени
    /// </summary>
    public static string FormatDateTime(DateTime dateTime, string format = "yyyy-MM-dd HH:mm:ss")
    {
        return dateTime.ToString(format);
    }

    /// <summary>
    /// Вычисление возраста на основе даты рождения
    /// </summary>
    public static int CalculateAge(DateTime birthDate, DateTime? asOfDate = null)
    {
        var referenceDate = asOfDate ?? DateTime.UtcNow;
        var age = referenceDate.Year - birthDate.Year;
        if (referenceDate < birthDate.AddYears(age))
            age--;
        return age;
    }

    /// <summary>
    /// Проверка, является ли дата рабочим днём (пн-пт)
    /// </summary>
    public static bool IsWeekday(DateTime date)
    {
        return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
    }

    /// <summary>
    /// Получение следующей даты, соответствующей указанному дню недели
    /// </summary>
    public static DateTime GetNextWeekday(DateTime startDate, DayOfWeek targetDay)
    {
        var daysToAdd = ((int)targetDay - (int)startDate.DayOfWeek + 7) % 7;
        return startDate.AddDays(daysToAdd == 0 ? 7 : daysToAdd);
    }
}
