using Microsoft.AspNetCore.Mvc;
using RaporServisi.Application.DTOs;
using RaporServisi.Application.Services;
using System.ComponentModel.DataAnnotations;

namespace RaporServisi.Api.Controllers;

[ApiController]
[Route("api/v2/sgk")]
public class SgkReportsV2Controller : ControllerBase
{
    private readonly ISgkReportService _sgkReportService;
    private readonly ILogger<SgkReportsV2Controller> _logger;

    public SgkReportsV2Controller(ISgkReportService sgkReportService, ILogger<SgkReportsV2Controller> logger)
    {
        _sgkReportService = sgkReportService;
        _logger = logger;
    }

    /// <summary>
    /// SGK WS Login - Test endpoint
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] SgkLoginRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _sgkReportService.LoginAsync(request);

            if (result.Success)
                return Ok(result);

            return StatusCode(502, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login endpoint hatası");
            return StatusCode(500, new { message = "Sistem hatası", error = ex.Message });
        }
    }

    /// <summary>
    /// Tarih ile rapor arama (METOT 2 - RaporAramaTarihile)
    /// </summary>
    [HttpPost("reports/search-by-date")]
    public async Task<IActionResult> SearchReportsByDate([FromBody] RaporAramaRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _sgkReportService.RaporAramaTarihileAsync(request);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RaporAramaTarihile endpoint hatası");
            return StatusCode(500, new { message = "Sistem hatası", error = ex.Message });
        }
    }

    /// <summary>
    /// Onaylı raporlar listesi (METOT 8 - OnayliRaporlarTarihile)
    /// 5 yıl öncesinden itibaren onaylanmış raporları getirir
    /// </summary>
    [HttpPost("reports/approved")]
    public async Task<IActionResult> GetApprovedReports([FromBody] OnayliRaporlarRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _sgkReportService.OnayliRaporlarTarihileAsync(request);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OnayliRaporlarTarihile endpoint hatası");
            return StatusCode(500, new { message = "Sistem hatası", error = ex.Message });
        }
    }

    /// <summary>
    /// Raporu okundu olarak işaretle ve kapat (METOT 9 - RaporOkunduKapat)
    /// ÖNEMLİ: Bu işlem yapılmazsa diğer raporlara erişim engellenir
    /// </summary>
    [HttpPost("reports/mark-as-read")]
    public async Task<IActionResult> MarkReportAsRead([FromBody] RaporOkunduKapatRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _sgkReportService.RaporOkunduKapatAsync(request);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RaporOkunduKapat endpoint hatası");
            return StatusCode(500, new { message = "Sistem hatası", error = ex.Message });
        }
    }

    /// <summary>
    /// Rapor onaylama - Çalışmazlık beyanı (METOT 4 - RaporOnay)
    /// </summary>
    [HttpPost("reports/approve")]
    public async Task<IActionResult> ApproveReport([FromBody] RaporOnayRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _sgkReportService.RaporOnayAsync(request);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RaporOnay endpoint hatası");
            return StatusCode(500, new { message = "Sistem hatası", error = ex.Message });
        }
    }

    /// <summary>
    /// Kapsamlı rapor sorgusu - Hem güncel hem 5 yıl öncesi raporları tek seferde getirir
    /// </summary>
    [HttpPost("reports/comprehensive")]
    public async Task<IActionResult> GetComprehensiveReports([FromBody] ComprehensiveRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var raporAramaRequest = new RaporAramaRequestDto
            {
                KullaniciAdi = request.KullaniciAdi,
                IsyeriKodu = request.IsyeriKodu,
                WsSifre = request.WsSifre,
                Tarih = request.Tarih
            };

            var result = await _sgkReportService.GetComprehensiveReportsAsync(raporAramaRequest, request.IncludeHistorical);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Comprehensive reports endpoint hatası");
            return StatusCode(500, new { message = "Sistem hatası", error = ex.Message });
        }
    }

    /// <summary>
    /// Toplu rapor kapatma - Birden fazla raporu tek seferde kapat
    /// </summary>
    [HttpPost("reports/bulk-close")]
    public async Task<IActionResult> BulkCloseReports([FromBody] BulkCloseRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (request.MedulaRaporIds == null || !request.MedulaRaporIds.Any())
            return BadRequest("En az bir rapor ID gereklidir");

        try
        {
            var results = new List<object>();
            var successCount = 0;

            foreach (var raporId in request.MedulaRaporIds)
            {
                var closeRequest = new RaporOkunduKapatRequestDto
                {
                    KullaniciAdi = request.KullaniciAdi,
                    IsyeriKodu = request.IsyeriKodu,
                    WsSifre = request.WsSifre,
                    MedulaRaporId = raporId
                };

                var result = await _sgkReportService.RaporOkunduKapatAsync(closeRequest);

                results.Add(new
                {
                    raporId = raporId,
                    success = result.Success,
                    message = result.Message
                });

                if (result.Success) successCount++;

                // Kısa bekleme
                await Task.Delay(100);
            }

            return Ok(new
            {
                success = true,
                message = $"{successCount}/{request.MedulaRaporIds.Count()} rapor başarıyla kapatıldı",
                results = results,
                totalProcessed = request.MedulaRaporIds.Count(),
                successCount = successCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BulkClose endpoint hatası");
            return StatusCode(500, new { message = "Sistem hatası", error = ex.Message });
        }
    }
}

// Ek DTOs Controller için
public class ComprehensiveRequestDto : RaporAramaRequestDto
{
    public bool IncludeHistorical { get; set; } = true;
}

public class BulkCloseRequestDto : SgkLoginRequestDto
{
    [Required]
    public IEnumerable<long> MedulaRaporIds { get; set; } = new List<long>();
}