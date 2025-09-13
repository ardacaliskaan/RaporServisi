namespace RaporServisi.Application.Utilities;

public static class DateHelper
{
    // SGK tarih formatı
    private const string SgkDateFormat = "dd.MM.yyyy";

    public static (string startDate, string endDate) CalculateApprovedReportsDateRange(string inputDate)
    {
        if (!DateTime.TryParseExact(inputDate, SgkDateFormat, null,
            System.Globalization.DateTimeStyles.None, out var date))
        {
            throw new ArgumentException($"Invalid date format. Expected: {SgkDateFormat}");
        }

        var startDate = date.AddYears(-5);
        return (FormatToSgkDate(startDate), inputDate);
    }

    /// <summary>
    /// - Başlangıç tarihi yoksa → bitiş tarihi - 5 yıl
    /// - Bitiş tarihi yoksa → bugün - 5 yıl başlangıç tarihi
    /// </summary>
    public static (string startDate, string endDate) ApplyDefaultDateLogic(string? startDate, string? endDate)
    {
        var today = DateTime.Today;

        // Her iki tarih de var - hiçbir şey yapma
        if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
        {
            ValidateDateRange(startDate, endDate);
            return (startDate, endDate);
        }

        // Sadece bitiş tarihi var - başlangıç = bitiş - 5 yıl
        if (string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
        {
            if (!DateTime.TryParseExact(endDate, SgkDateFormat, null,
                System.Globalization.DateTimeStyles.None, out var endDateTime))
            {
                throw new ArgumentException($"Invalid end date format. Expected: {SgkDateFormat}");
            }

            var calculatedStart = endDateTime.AddYears(-5);
            return (FormatToSgkDate(calculatedStart), endDate);
        }

        // Sadece başlangıç tarihi var - bitiş = bugün, başlangıç kontrol et
        if (!string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
        {
            ValidateDateFormat(startDate);
            return (startDate, FormatToSgkDate(today));
        }

        // Hiçbir tarih yok - başlangıç = bugün - 5 yıl, bitiş = bugün
        var defaultStart = today.AddYears(-5);
        return (FormatToSgkDate(defaultStart), FormatToSgkDate(today));
    }

    /// <summary>
    /// Tek tarih girildiyinde otomatik 5 yıl geriye hesaplama
    /// Notlarda: "OnaylıRaporlarTarihile (girilen tarih - 5 yıl yapılacak)"
    /// </summary>
    public static (string startDate, string endDate) CalculateFiveYearRange(string inputDate)
    {
        if (!DateTime.TryParseExact(inputDate, SgkDateFormat, null,
            System.Globalization.DateTimeStyles.None, out var date))
        {
            throw new ArgumentException($"Invalid date format. Expected: {SgkDateFormat}");
        }

        var startDate = date.AddYears(-5);
        return (FormatToSgkDate(startDate), inputDate);
    }

    /// <summary>
    /// Tarih aralığı validasyonu
    /// </summary>
    public static void ValidateDateRange(string startDate, string endDate)
    {
        if (!DateTime.TryParseExact(startDate, SgkDateFormat, null,
            System.Globalization.DateTimeStyles.None, out var start))
        {
            throw new ArgumentException($"Invalid start date format. Expected: {SgkDateFormat}");
        }

        if (!DateTime.TryParseExact(endDate, SgkDateFormat, null,
            System.Globalization.DateTimeStyles.None, out var end))
        {
            throw new ArgumentException($"Invalid end date format. Expected: {SgkDateFormat}");
        }

        if (start > end)
        {
            throw new ArgumentException("Start date cannot be greater than end date");
        }

        if (end > DateTime.Today)
        {
            throw new ArgumentException("End date cannot be in the future");
        }

        // SGK kuralı: Maksimum 1 ay fark (bazı metodlarda)
        var daysDifference = (end - start).TotalDays;
        if (daysDifference > 365) // 1 yıldan fazla olamaz
        {
            throw new ArgumentException("Date range cannot exceed 1 year");
        }
    }

    /// <summary>
    /// Tek tarih validasyonu
    /// </summary>
    public static void ValidateDateFormat(string date)
    {
        if (!DateTime.TryParseExact(date, SgkDateFormat, null,
            System.Globalization.DateTimeStyles.None, out var parsedDate))
        {
            throw new ArgumentException($"Invalid date format. Expected: {SgkDateFormat}");
        }

        if (parsedDate > DateTime.Today)
        {
            throw new ArgumentException("Date cannot be in the future");
        }
    }

    /// <summary>
    /// DateTime'ı SGK formatına çevir
    /// </summary>
    public static string FormatToSgkDate(DateTime date)
    {
        return date.ToString(SgkDateFormat);
    }

    /// <summary>
    /// SGK formatından DateTime'a çevir
    /// </summary>
    public static DateTime ParseSgkDate(string sgkDate)
    {
        if (string.IsNullOrWhiteSpace(sgkDate))
        {
            return DateTime.MinValue;
        }

        if (DateTime.TryParseExact(sgkDate, SgkDateFormat, null,
            System.Globalization.DateTimeStyles.None, out var result))
        {
            return result;
        }

        return DateTime.MinValue;
    }

    /// <summary>
    /// Güvenli SGK tarih parsing - hata fırlatmaz
    /// </summary>
    public static DateTime? TryParseSgkDate(string? sgkDate)
    {
        if (string.IsNullOrWhiteSpace(sgkDate))
        {
            return null;
        }

        if (DateTime.TryParseExact(sgkDate, SgkDateFormat, null,
            System.Globalization.DateTimeStyles.None, out var result))
        {
            return result;
        }

        return null;
    }

    /// <summary>
    /// Bugünden X yıl geriye tarihi hesapla
    /// </summary>
    public static string GetDateYearsAgo(int years)
    {
        var date = DateTime.Today.AddYears(-years);
        return FormatToSgkDate(date);
    }

    /// <summary>
    /// Belirtilen tarihten X yıl geriye tarihi hesapla
    /// </summary>
    public static string GetDateYearsAgo(string baseDate, int years)
    {
        var date = ParseSgkDate(baseDate);
        if (date == DateTime.MinValue)
        {
            throw new ArgumentException($"Invalid base date format. Expected: {SgkDateFormat}");
        }

        var resultDate = date.AddYears(-years);
        return FormatToSgkDate(resultDate);
    }

    /// <summary>
    /// İki tarih arasındaki gün farkını hesapla
    /// </summary>
    public static int GetDaysDifference(string startDate, string endDate)
    {
        var start = ParseSgkDate(startDate);
        var end = ParseSgkDate(endDate);

        if (start == DateTime.MinValue || end == DateTime.MinValue)
        {
            return 0;
        }

        return (int)(end - start).TotalDays;
    }

    /// <summary>
    /// Tarih string'inin SGK formatında olup olmadığını kontrol et
    /// </summary>
    public static bool IsValidSgkDateFormat(string? date)
    {
        if (string.IsNullOrWhiteSpace(date))
        {
            return false;
        }

        return DateTime.TryParseExact(date, SgkDateFormat, null,
            System.Globalization.DateTimeStyles.None, out _);
    }

    /// <summary>
    /// Bugünün SGK formatında string'ini al
    /// </summary>
    public static string Today => FormatToSgkDate(DateTime.Today);

    /// <summary>
    /// 5 yıl öncenin SGK formatında string'ini al
    /// </summary>
    public static string FiveYearsAgo => GetDateYearsAgo(5);
}