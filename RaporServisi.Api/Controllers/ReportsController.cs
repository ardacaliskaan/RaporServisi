using Microsoft.AspNetCore.Mvc;
using RaporServisi.Application.DTOs;
using RaporServisi.Application.Services;
using System.ComponentModel.DataAnnotations;

namespace RaporServisi.Api.Controllers;

/// <summary>
/// Reports Controller - English naming with Success/Error list patterns
/// Implements all requirements from notes: bulk operations, auto date calculations
/// </summary>
[ApiController]
[Route("api/v1/reports")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    /// <summary>
    /// SGK Login - Test endpoint for credential validation
    /// Returns 30-minute valid token
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Login result with token</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponseDto<LoginResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<LoginResponseDto>), 400)]
    [ProducesResponseType(typeof(ApiResponseDto<LoginResponseDto>), 502)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ApiResponseDto<LoginResponseDto>
            {
                Success = false,
                Message = "Validation failed",
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
            });

        try
        {
            var result = await _reportService.LoginAsync(request);

            if (result.Success && result.Data?.Success == true)
                return Ok(result);

            return StatusCode(502, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login endpoint error");
            return StatusCode(500, new ApiResponseDto<LoginResponseDto>
            {
                Success = false,
                Message = "System error occurred",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Search Reports by Date
    /// Returns Success/Error list pattern as specified in notes
    /// Each request creates new wsLogin token
    /// </summary>
    /// <param name="request">Search criteria with date</param>
    /// <returns>Reports grouped by success/error lists</returns>
    [HttpPost("search")]
    [ProducesResponseType(typeof(ApiResponseDto<ReportSearchResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<ReportSearchResponseDto>), 400)]
    public async Task<IActionResult> SearchReportsByDate([FromBody] ReportSearchRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(CreateValidationErrorResponse<ReportSearchResponseDto>());

        try
        {
            var result = await _reportService.SearchReportsByDateAsync(request);

            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Search reports endpoint error");
            return StatusCode(500, CreateSystemErrorResponse<ReportSearchResponseDto>(ex.Message));
        }
    }

    /// <summary>
    /// Get Approved Reports with Auto Date Calculation
    /// Notes rule: "girilen tarih - 5 yıl yapılacak"
    /// Default date logic: if start/end dates missing, auto-calculate
    /// Returns Success/Error list pattern
    /// </summary>
    /// <param name="request">Date criteria (auto-calculates -5 years)</param>
    /// <returns>Approved reports grouped by success/error lists</returns>
    [HttpPost("approved")]
    [ProducesResponseType(typeof(ApiResponseDto<ApprovedReportsResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<ApprovedReportsResponseDto>), 400)]
    public async Task<IActionResult> GetApprovedReports([FromBody] ApprovedReportsRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(CreateValidationErrorResponse<ApprovedReportsResponseDto>());

        try
        {
            var result = await _reportService.GetApprovedReportsAsync(request);

            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get approved reports endpoint error");
            return StatusCode(500, CreateSystemErrorResponse<ApprovedReportsResponseDto>(ex.Message));
        }
    }

    /// <summary>
    /// Close Single Report
    /// Important: Must be called to mark report as read, otherwise access to other reports blocked
    /// </summary>
    /// <param name="request">Report close request</param>
    /// <returns>Operation result</returns>
    [HttpPost("close")]
    [ProducesResponseType(typeof(ApiResponseDto<ReportOperationResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<ReportOperationResponseDto>), 400)]
    public async Task<IActionResult> CloseReport([FromBody] CloseReportRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(CreateValidationErrorResponse<ReportOperationResponseDto>());

        try
        {
            var result = await _reportService.CloseReportAsync(request);

            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Close report endpoint error - ReportId: {ReportId}", request.ReportId);
            return StatusCode(500, CreateSystemErrorResponse<ReportOperationResponseDto>(ex.Message));
        }
    }

    /// <summary>
    /// Approve Single Report
    /// Creates work disability notification
    /// </summary>
    /// <param name="request">Report approval request</param>
    /// <returns>Operation result</returns>
    [HttpPost("approve")]
    [ProducesResponseType(typeof(ApiResponseDto<ReportOperationResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<ReportOperationResponseDto>), 400)]
    public async Task<IActionResult> ApproveReport([FromBody] ReportApprovalRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(CreateValidationErrorResponse<ReportOperationResponseDto>());

        try
        {
            var result = await _reportService.ApproveReportAsync(request);

            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Approve report endpoint error - ReportId: {ReportId}", request.ReportId);
            return StatusCode(500, CreateSystemErrorResponse<ReportOperationResponseDto>(ex.Message));
        }
    }

    /// <summary>
    /// Cancel Report Approval
    /// Cancels work disability notification
    /// </summary>
    /// <param name="request">Report cancellation request</param>
    /// <returns>Operation result</returns>
    [HttpPost("cancel-approval")]
    [ProducesResponseType(typeof(ApiResponseDto<ReportOperationResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<ReportOperationResponseDto>), 400)]
    public async Task<IActionResult> CancelReportApproval([FromBody] ReportCancellationRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(CreateValidationErrorResponse<ReportOperationResponseDto>());

        try
        {
            var result = await _reportService.CancelReportApprovalAsync(request);

            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cancel approval endpoint error - ReportId: {ReportId}", request.ReportId);
            return StatusCode(500, CreateSystemErrorResponse<ReportOperationResponseDto>(ex.Message));
        }
    }

    // BULK OPERATIONS - Notes requirement: "Birden fazla personel onaylama desteği"

    /// <summary>
    /// Bulk Report Approval
    /// Notes requirement: "Birden fazla personel onaylama desteği"
    /// Returns Success/Error list pattern: "dönüş olarak succes list ve error list olucak"
    /// </summary>
    /// <param name="request">Multiple report approval request</param>
    /// <returns>Bulk operation results with success/error lists</returns>
    [HttpPost("bulk-approve")]
    [ProducesResponseType(typeof(ApiResponseDto<BulkReportApprovalResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<BulkReportApprovalResponseDto>), 400)]
    public async Task<IActionResult> BulkApproveReports([FromBody] BulkReportApprovalRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(CreateValidationErrorResponse<BulkReportApprovalResponseDto>());

        if (request.Reports == null || !request.Reports.Any())
            return BadRequest(new ApiResponseDto<BulkReportApprovalResponseDto>
            {
                Success = false,
                Message = "At least one report required for bulk approval",
                Errors = new List<string> { "Reports list cannot be empty" }
            });

        try
        {
            var result = await _reportService.BulkApproveReportsAsync(request);

            return Ok(result); // Always return 200 for bulk operations, success/error info in response data
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bulk approve reports endpoint error");
            return StatusCode(500, CreateSystemErrorResponse<BulkReportApprovalResponseDto>(ex.Message));
        }
    }

    /// <summary>
    /// Bulk Report Approval Cancellation
    /// Notes requirement: "raporiptal toplu şekilde yapılabilecek"
    /// Returns Success/Error list pattern
    /// </summary>
    /// <param name="request">Multiple report cancellation request</param>
    /// <returns>Bulk operation results with success/error lists</returns>
    [HttpPost("bulk-cancel-approvals")]
    [ProducesResponseType(typeof(ApiResponseDto<BulkReportCancellationResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<BulkReportCancellationResponseDto>), 400)]
    public async Task<IActionResult> BulkCancelReportApprovals([FromBody] BulkReportCancellationRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(CreateValidationErrorResponse<BulkReportCancellationResponseDto>());

        if (request.ReportIds == null || !request.ReportIds.Any())
            return BadRequest(new ApiResponseDto<BulkReportCancellationResponseDto>
            {
                Success = false,
                Message = "At least one report ID required for bulk cancellation",
                Errors = new List<string> { "ReportIds list cannot be empty" }
            });

        try
        {
            var result = await _reportService.BulkCancelReportApprovalsAsync(request);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bulk cancel approvals endpoint error");
            return StatusCode(500, CreateSystemErrorResponse<BulkReportCancellationResponseDto>(ex.Message));
        }
    }

    /// <summary>
    /// Bulk Close Reports
    /// Closes multiple reports at once
    /// Returns Success/Error list pattern
    /// </summary>
    /// <param name="request">Multiple report close request</param>
    /// <returns>Bulk operation results with success/error lists</returns>
    [HttpPost("bulk-close")]
    [ProducesResponseType(typeof(ApiResponseDto<BulkCloseReportResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<BulkCloseReportResponseDto>), 400)]
    public async Task<IActionResult> BulkCloseReports([FromBody] BulkCloseReportRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(CreateValidationErrorResponse<BulkCloseReportResponseDto>());

        if (request.ReportIds == null || !request.ReportIds.Any())
            return BadRequest(new ApiResponseDto<BulkCloseReportResponseDto>
            {
                Success = false,
                Message = "At least one report ID required for bulk close",
                Errors = new List<string> { "ReportIds list cannot be empty" }
            });

        try
        {
            var result = await _reportService.BulkCloseReportsAsync(request);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bulk close reports endpoint error");
            return StatusCode(500, CreateSystemErrorResponse<BulkCloseReportResponseDto>(ex.Message));
        }
    }

    // UTILITY ENDPOINTS

    /// <summary>
    /// Validate SGK Credentials
    /// Test credentials without performing operations
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Validation result</returns>
    [HttpPost("validate-credentials")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), 400)]
    public async Task<IActionResult> ValidateCredentials([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(CreateValidationErrorResponse<bool>());

        try
        {
            var result = await _reportService.ValidateCredentialsAsync(request);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Validate credentials endpoint error");
            return StatusCode(500, CreateSystemErrorResponse<bool>(ex.Message));
        }
    }

    /// <summary>
    /// Get Comprehensive Reports
    /// Combines search reports + approved reports with auto date calculation
    /// </summary>
    /// <param name="request">Search criteria</param>
    /// <param name="includeApprovedReports">Include 5-year approved reports</param>
    /// <returns>Combined report data</returns>
    [HttpPost("comprehensive")]
    [ProducesResponseType(typeof(ApiResponseDto<ComprehensiveReportResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<ComprehensiveReportResponseDto>), 400)]
    public async Task<IActionResult> GetComprehensiveReports(
        [FromBody] ReportSearchRequestDto request,
        [FromQuery] bool includeApprovedReports = true)
    {
        if (!ModelState.IsValid)
            return BadRequest(CreateValidationErrorResponse<ComprehensiveReportResponseDto>());

        try
        {
            var result = await _reportService.GetComprehensiveReportsAsync(request, includeApprovedReports);

            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Comprehensive reports endpoint error");
            return StatusCode(500, CreateSystemErrorResponse<ComprehensiveReportResponseDto>(ex.Message));
        }
    }

    // HELPER METHODS

    private ApiResponseDto<T> CreateValidationErrorResponse<T>()
    {
        return new ApiResponseDto<T>
        {
            Success = false,
            Message = "Validation errors occurred",
            Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
        };
    }

    private ApiResponseDto<T> CreateSystemErrorResponse<T>(string errorMessage)
    {
        return new ApiResponseDto<T>
        {
            Success = false,
            Message = "System error occurred",
            Errors = new List<string> { errorMessage }
        };
    }
}