using System.ComponentModel.DataAnnotations;

namespace RaporServisi.Application.DTOs;

// Base Request DTO - Tüm istekler için ortak alanlar
public abstract class BaseRequestDto
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Company code is required")]
    [StringLength(10, ErrorMessage = "Company code cannot exceed 10 characters")]
    public string CompanyCode { get; set; } = "";

    [Required(ErrorMessage = "WS password is required")]
    [StringLength(50, ErrorMessage = "WS password cannot exceed 50 characters")]
    public string WsPassword { get; set; } = "";
}

// Login Request - Test endpoint için
public class LoginRequestDto : BaseRequestDto
{
    // BaseRequestDto'dan inherit edilenler yeterli
}

// Report Search Request - Ana rapor arama
public class ReportSearchRequestDto : BaseRequestDto
{
    [Required(ErrorMessage = "Date is required")]
    [RegularExpression(@"^\d{2}\.\d{2}\.\d{4}$", ErrorMessage = "Date format must be dd.MM.yyyy")]
    public string Date { get; set; } = "";
}

// Approved Reports Request - Onaylı raporlar için (Otomatik -5 yıl hesaplama)
public class ApprovedReportsRequestDto : BaseRequestDto
{
    [Required(ErrorMessage = "Date is required")]
    [RegularExpression(@"^\d{2}\.\d{2}\.\d{4}$", ErrorMessage = "Date format must be dd.MM.yyyy")]
    public string Date { get; set; } = "";

    // İsteğe bağlı - belirtilmezse otomatik Date - 5 yıl hesaplanır
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
}

// Close Report Request - Rapor kapatma
public class CloseReportRequestDto : BaseRequestDto
{
    [Required(ErrorMessage = "Report ID is required")]
    [Range(1, long.MaxValue, ErrorMessage = "Valid report ID required")]
    public long ReportId { get; set; }
}

// Single Report Approval Request - Tek rapor onay
public class ReportApprovalRequestDto : BaseRequestDto
{
    [Required(ErrorMessage = "TC identity number is required")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "TC identity number must be 11 characters")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "TC identity number must be numeric")]
    public string TcIdentityNumber { get; set; } = "";

    [Required(ErrorMessage = "Case type is required")]
    [RegularExpression("^[1-4]$", ErrorMessage = "Case: 1-Work Accident, 2-Occupational Disease, 3-Illness, 4-Maternity")]
    public string CaseType { get; set; } = "";

    [Required(ErrorMessage = "Report ID is required")]
    [Range(1, long.MaxValue, ErrorMessage = "Valid report ID required")]
    public long ReportId { get; set; }

    [Required(ErrorMessage = "Status is required")]
    [RegularExpression("^[01]$", ErrorMessage = "Status: 0-Did Not Work, 1-Worked")]
    public string Status { get; set; } = "";

    [Required(ErrorMessage = "Date is required")]
    [RegularExpression(@"^\d{2}\.\d{2}\.\d{4}$", ErrorMessage = "Date format must be dd.MM.yyyy")]
    public string Date { get; set; } = "";
}

// Single Report Cancellation Request - Tek rapor iptal
public class ReportCancellationRequestDto : BaseRequestDto
{
    [Required(ErrorMessage = "Report ID is required")]
    [Range(1, long.MaxValue, ErrorMessage = "Valid report ID required")]
    public long ReportId { get; set; }
}

// Bulk Report Approval Request - Toplu rapor onay
public class BulkReportApprovalRequestDto : BaseRequestDto
{
    [Required(ErrorMessage = "At least one report required")]
    [MinLength(1, ErrorMessage = "At least one report must be provided")]
    [MaxLength(50, ErrorMessage = "Maximum 50 reports can be processed at once")]
    public List<ReportApprovalItemDto> Reports { get; set; } = new();
}

// Bulk Report Cancellation Request - Toplu rapor iptal
public class BulkReportCancellationRequestDto : BaseRequestDto
{
    [Required(ErrorMessage = "At least one report ID required")]
    [MinLength(1, ErrorMessage = "At least one report ID must be provided")]
    [MaxLength(50, ErrorMessage = "Maximum 50 reports can be processed at once")]
    public List<long> ReportIds { get; set; } = new();
}

// Bulk Report Close Request - Toplu rapor kapatma
public class BulkCloseReportRequestDto : BaseRequestDto
{
    [Required(ErrorMessage = "At least one report ID required")]
    [MinLength(1, ErrorMessage = "At least one report ID must be provided")]
    [MaxLength(100, ErrorMessage = "Maximum 100 reports can be processed at once")]
    public List<long> ReportIds { get; set; } = new();
}