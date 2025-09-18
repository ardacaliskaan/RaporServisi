using RaporServisi.Application.DTOs;

namespace RaporServisi.Application.Services;

/// <summary>
/// Report Service Interface - English naming with Success/Error list patterns
/// </summary>
public interface IReportService
{
    /// <summary>
    /// SGK Login - Test endpoint, returns token
    /// </summary>
    Task<ApiResponseDto<LoginResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Search Reports by Date - RaporAramaTarihile
    /// Returns Success/Error list pattern
    /// </summary>
    Task<ApiResponseDto<ReportSearchResponseDto>> SearchReportsByDateAsync(
        ReportSearchRequestDto request,
        CancellationToken ct = default);

    /// <summary>
    /// Get Approved Reports - OnaylıRaporlarTarihile
    /// Notlardaki kural: "girilen tarih - 5 yıl yapılacak"
    /// Auto date calculation: Date - 5 years
    /// Returns Success/Error list pattern
    /// </summary>
    Task<ApiResponseDto<ApprovedReportsResponseDto>> GetApprovedReportsAsync(
        ApprovedReportsRequestDto request,
        CancellationToken ct = default);

    /// <summary>
    /// Close Report - RaporOkunduKapat
    /// Single report close operation
    /// </summary>
    Task<ApiResponseDto<ReportOperationResponseDto>> CloseReportAsync(
        CloseReportRequestDto request,
        CancellationToken ct = default);

    /// <summary>
    /// Approve Report - RaporOnay
    /// Single report approval operation
    /// </summary>
    Task<ApiResponseDto<ReportOperationResponseDto>> ApproveReportAsync(
        ReportApprovalRequestDto request,
        CancellationToken ct = default);

    /// <summary>
    /// Cancel Report Approval - OnaylIptal
    /// Single report cancellation operation
    /// </summary>
    Task<ApiResponseDto<ReportOperationResponseDto>> CancelReportApprovalAsync(
        ReportCancellationRequestDto request,
        CancellationToken ct = default);

    // BULK OPERATIONS - Notlarda belirtilen toplu işlemler

    /// <summary>
    /// Bulk Report Approval - RaporOnay (Multiple)
    /// Notlardaki gereksinim: "Birden fazla personel onaylama desteği"
    /// Returns Success/Error list pattern
    /// </summary>
    Task<ApiResponseDto<BulkReportApprovalResponseDto>> BulkApproveReportsAsync(
        BulkReportApprovalRequestDto request,
        CancellationToken ct = default);

    /// <summary>
    /// Bulk Report Cancellation - OnaylIptal (Multiple)  
    /// Notlardaki gereksinim: "raporiptal toplu şekilde yapılabilecek"
    /// Returns Success/Error list pattern
    /// </summary>
    Task<ApiResponseDto<BulkReportCancellationResponseDto>> BulkCancelReportApprovalsAsync(
        BulkReportCancellationRequestDto request,
        CancellationToken ct = default);

    /// <summary>
    /// Bulk Close Reports - RaporOkunduKapat (Multiple)
    /// Returns Success/Error list pattern
    /// </summary>
    Task<ApiResponseDto<BulkCloseReportResponseDto>> BulkCloseReportsAsync(
        BulkCloseReportRequestDto request,
        CancellationToken ct = default);

    // COMPREHENSIVE OPERATIONS

    /// <summary>
    /// Comprehensive Report Search - Combination of multiple operations
    /// Gets both current reports and approved reports in one call
    /// Uses auto date calculations from notes
    /// </summary>
    Task<ApiResponseDto<ComprehensiveReportResponseDto>> GetComprehensiveReportsAsync(
        ReportSearchRequestDto request,
        bool includeApprovedReports = true,
        CancellationToken ct = default);

    // UTILITY METHODS

    /// <summary>
    /// Validate SGK credentials without performing operations
    /// </summary>
    Task<ApiResponseDto<bool>> ValidateCredentialsAsync(
        LoginRequestDto request,
        CancellationToken ct = default);

    /// <summary>
    /// Get Report Statistics - Summary information
    /// </summary>
    Task<ApiResponseDto<ReportStatisticsDto>> GetReportStatisticsAsync(
        ReportSearchRequestDto request,
        CancellationToken ct = default);

    /// <summary>
    /// Auto-process reports - Search and auto-close read reports
    /// Useful for automation scenarios
    /// </summary>
    Task<ApiResponseDto<AutoProcessResultDto>> AutoProcessReportsAsync(
        ReportSearchRequestDto request,
        bool autoClose = false,
        CancellationToken ct = default);
}

// Additional Response DTOs for comprehensive operations
public class ComprehensiveReportResponseDto : SuccessErrorListResponseDto<ReportItemDto, ReportErrorDto>
{
    public ReportSearchResponseDto? CurrentReports { get; set; }
    public ApprovedReportsResponseDto? ApprovedReports { get; set; }
    public List<string> Warnings { get; set; } = new();
    public bool UsedAutoDateCalculation { get; set; }
    public string SearchCriteria { get; set; } = "";
}

public class AutoProcessResultDto
{
    public int ReportsFound { get; set; }
    public int ReportsProcessed { get; set; }
    public int ReportsClosed { get; set; }
    public int ReportsWithErrors { get; set; }
    public List<ReportItemDto> ProcessedReports { get; set; } = new();
    public List<ReportErrorDto> Errors { get; set; } = new();
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan ProcessingDuration { get; set; }
}