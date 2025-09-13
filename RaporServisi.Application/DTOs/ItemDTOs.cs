namespace RaporServisi.Application.DTOs;

// Report Item DTO - RaporAramaTarihile için
public class ReportItemDto
{
    public string TcIdentityNumber { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string FullName => $"{FirstName?.Trim()} {LastName?.Trim()}".Trim();
    public long ReportId { get; set; }
    public string ReportTrackingNumber { get; set; } = "";
    public string ReportSequenceNumber { get; set; } = "";

    // Tarih alanları
    public DateTime? ClinicDate { get; set; }
    public DateTime? InpatientStartDate { get; set; }
    public DateTime? InpatientEndDate { get; set; }
    public DateTime? OutpatientStartDate { get; set; }
    public DateTime? OutpatientEndDate { get; set; }
    public DateTime? WorkControlDate { get; set; }
    public DateTime? PregnancyStartDate { get; set; }
    public DateTime? ReportEndDate { get; set; }
    public DateTime? AccidentDate { get; set; }

    // Vaka bilgileri
    public string CaseCode { get; set; } = "";
    public string CaseName { get; set; } = "";
    public string ReportStatus { get; set; } = "";
    public string ReportStatusName { get; set; } = "";

    // Tesis bilgileri
    public string FacilityCode { get; set; } = "";
    public string FacilityName { get; set; } = "";

    // Diğer
    public string? Archive { get; set; }
    public bool IsArchived => !string.IsNullOrEmpty(Archive);

    // Helper properties
    public string CaseTypeDescription => GetCaseTypeDescription(CaseCode);
    public string ReportStatusDescription => GetReportStatusDescription(ReportStatus);
}

// Approved Report Item DTO - OnaylıRaporlarTarihile için
public class ApprovedReportItemDto
{
    public string TcIdentityNumber { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string FullName => $"{FirstName?.Trim()} {LastName?.Trim()}".Trim();
    public long ReportId { get; set; }
    public string ReportTrackingNumber { get; set; } = "";
    public string ReportSequenceNumber { get; set; } = "";

    // Onaylı raporlara özel alanlar
    public DateTime? ClinicDate { get; set; }
    public DateTime? WorkControlDate { get; set; }
    public DateTime? AccidentDate { get; set; }

    // Vaka bilgileri
    public string CaseCode { get; set; } = "";
    public string CaseName { get; set; } = "";

    // Onay durumu
    public DateTime? ApprovalDate { get; set; }
    public bool IsApproved => ApprovalDate.HasValue;

    // Helper properties
    public string CaseTypeDescription => GetCaseTypeDescription(CaseCode);
}

// Report Approval Item DTO - Toplu onay işlemleri için
public class ReportApprovalItemDto
{
    public string TcIdentityNumber { get; set; } = "";
    public string CaseType { get; set; } = "";
    public long ReportId { get; set; }
    public string Status { get; set; } = "";
    public string Date { get; set; } = "";

    // Validation için ek alanlar
    public string? EmployeeName { get; set; }
    public string? ReportDescription { get; set; }

    // Helper properties
    public string StatusDescription => Status switch
    {
        "0" => "Did Not Work",
        "1" => "Worked",
        _ => "Unknown"
    };

    public string CaseTypeDescription => GetCaseTypeDescription(CaseType);
}

// Report Detail DTO - Rapor detayları için
public class ReportDetailDto
{
    public long ReportId { get; set; }
    public string NotificationId { get; set; } = "";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string WorkStatus { get; set; } = "";
    public DateTime? ProcessDate { get; set; }
    public string PaymentStatus { get; set; } = "";

    // Detay alanlar
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

// Employee Summary DTO - Çalışan özet bilgileri için
public class EmployeeSummaryDto
{
    public string TcIdentityNumber { get; set; } = "";
    public string FullName { get; set; } = "";
    public int TotalReports { get; set; }
    public int ApprovedReports { get; set; }
    public int PendingReports { get; set; }
    public DateTime? LastReportDate { get; set; }
    public List<string> CaseTypes { get; set; } = new();
}

// Report Statistics DTO - İstatistik bilgileri için
public class ReportStatisticsDto
{
    public int TotalReports { get; set; }
    public int ApprovedReports { get; set; }
    public int PendingReports { get; set; }
    public int RejectedReports { get; set; }
    public int ClosedReports { get; set; }

    // Vaka türü dağılımı
    public Dictionary<string, int> CaseTypeDistribution { get; set; } = new();

    // Tarih aralığı
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }

    // Yüzdelik dağılım
    public double ApprovalRate => TotalReports > 0 ? (double)ApprovedReports / TotalReports * 100 : 0;
    public double PendingRate => TotalReports > 0 ? (double)PendingReports / TotalReports * 100 : 0;
}

// Helper Methods - SGK tarih parsing metodları
public static class ItemDtoHelpers
{
    public static DateTime ParseSgkDate(string? sgkDate)
    {
        if (string.IsNullOrWhiteSpace(sgkDate)) return DateTime.MinValue;

        if (DateTime.TryParseExact(sgkDate, "dd.MM.yyyy", null,
            System.Globalization.DateTimeStyles.None, out var result))
        {
            return result;
        }

        return DateTime.MinValue;
    }

    public static string FormatToSgkDate(DateTime date)
    {
        return date.ToString("dd.MM.yyyy");
    }
}