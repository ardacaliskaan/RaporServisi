namespace RaporServisi.Application.DTOs;

// Base API Response - Tüm API yanıtları için
public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public T? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public List<string> Errors { get; set; } = new();
}

// Success/Error List Pattern - Notlarda belirtilen yapı
public class SuccessErrorListResponseDto<TSuccess, TError>
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public List<TSuccess> SuccessList { get; set; } = new();
    public List<TError> ErrorList { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Summary properties
    public int SuccessCount => SuccessList.Count;
    public int ErrorCount => ErrorList.Count;
    public int TotalProcessed => SuccessCount + ErrorCount;
}

// Login Response
public class LoginResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public string? Token { get; set; }
    public int ResultCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

// Report Search Response - Success/Error list pattern
public class ReportSearchResponseDto : SuccessErrorListResponseDto<ReportItemDto, ReportErrorDto>
{
    public string SearchDate { get; set; } = "";
    public string SearchCriteria { get; set; } = "";
}

// Approved Reports Response - Success/Error list pattern
public class ApprovedReportsResponseDto : SuccessErrorListResponseDto<ApprovedReportItemDto, ReportErrorDto>
{
    public string StartDate { get; set; } = "";
    public string EndDate { get; set; } = "";
    public string OriginalDate { get; set; } = "";
    public bool UsedAutoDateCalculation { get; set; }
}

// Single Report Operation Response
public class ReportOperationResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public long? ReportId { get; set; }
    public int ResultCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

// Bulk Report Approval Response - Success/Error list pattern
public class BulkReportApprovalResponseDto : SuccessErrorListResponseDto<ReportApprovalSuccessDto, ReportApprovalErrorDto>
{
    public int RequestedCount { get; set; }
    public List<string> Warnings { get; set; } = new();
}

// Bulk Report Cancellation Response - Success/Error list pattern  
public class BulkReportCancellationResponseDto : SuccessErrorListResponseDto<ReportCancellationSuccessDto, ReportCancellationErrorDto>
{
    public int RequestedCount { get; set; }
    public List<string> Warnings { get; set; } = new();
}

// Bulk Close Report Response - Success/Error list pattern
public class BulkCloseReportResponseDto : SuccessErrorListResponseDto<CloseReportSuccessDto, CloseReportErrorDto>
{
    public int RequestedCount { get; set; }
    public List<string> Warnings { get; set; } = new();
}

// Success DTOs - İşlem başarılı olanlar için
public class ReportApprovalSuccessDto
{
    public long ReportId { get; set; }
    public string TcIdentityNumber { get; set; } = "";
    public string EmployeeName { get; set; } = "";
    public string CaseType { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public string Message { get; set; } = "Report approved successfully";
}

public class ReportCancellationSuccessDto
{
    public long ReportId { get; set; }
    public string EmployeeName { get; set; } = "";
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public string Message { get; set; } = "Report cancellation successful";
}

public class CloseReportSuccessDto
{
    public long ReportId { get; set; }
    public string EmployeeName { get; set; } = "";
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public string Message { get; set; } = "Report closed successfully";
}

// Error DTOs - İşlem başarısız olanlar için
public class ReportErrorDto
{
    public string? TcIdentityNumber { get; set; }
    public string? EmployeeName { get; set; }
    public long? ReportId { get; set; }
    public int ErrorCode { get; set; }
    public string ErrorMessage { get; set; } = "";
    public DateTime ErrorTime { get; set; } = DateTime.UtcNow;
    public string? AdditionalInfo { get; set; }
}

public class ReportApprovalErrorDto
{
    public long ReportId { get; set; }
    public string TcIdentityNumber { get; set; } = "";
    public string CaseType { get; set; } = "";
    public int ErrorCode { get; set; }
    public string ErrorMessage { get; set; } = "";
    public DateTime ErrorTime { get; set; } = DateTime.UtcNow;
    public string? ValidationErrors { get; set; }
}

public class ReportCancellationErrorDto
{
    public long ReportId { get; set; }
    public int ErrorCode { get; set; }
    public string ErrorMessage { get; set; } = "";
    public DateTime ErrorTime { get; set; } = DateTime.UtcNow;
    public string? Reason { get; set; }
}

public class CloseReportErrorDto
{
    public long ReportId { get; set; }
    public int ErrorCode { get; set; }
    public string ErrorMessage { get; set; } = "";
    public DateTime ErrorTime { get; set; } = DateTime.UtcNow;
    public string? Reason { get; set; }
}

// SGK Operation Result - SGK servisinden dönen sonuçlar için
public class SgkOperationResultDto
{
    public bool Success { get; set; }
    public int ResultCode { get; set; }
    public string ResultMessage { get; set; } = "";
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public string? AdditionalData { get; set; }
}