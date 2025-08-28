using Microsoft.AspNetCore.Mvc;
using RaporServisi.Infrastructure.External;

namespace RaporServisi.Api.Controllers;

public record SgkReportQueryDto(string KullaniciAdi, string IsyeriKodu, string WsSifre, string Tarih);
public record SgkReportListItem(long MedulaRaporId, string Tckn, string? PoliklinikTarihi, int? VakaKodu, string? VakaAdi);

[ApiController]
[Route("api/v1/sgk/reports")]
public class SgkReportsController : ControllerBase
{
    private readonly SgkViziteDirectClient _client;
    public SgkReportsController(SgkViziteDirectClient client) => _client = client;

    [HttpPost] 
    public async Task<IActionResult> List([FromBody] SgkReportQueryDto dto, CancellationToken ct)
    {

        if (!DateTime.TryParse(dto.Tarih, out var date) &&
            !DateTime.TryParseExact(dto.Tarih, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out date))
        {
            return BadRequest("Tarih formatı 'yyyy-MM-dd' veya 'dd.MM.yyyy' olmalı.");
        }

        var cred = new ViziteLogin(dto.KullaniciAdi, dto.IsyeriKodu, dto.WsSifre);
        var items = await _client.GetReportsByDateAsync(cred, date, ct);

        var result = items.Select(x => new SgkReportListItem(
            x.MedulaRaporId,
            x.Tckn,
            x.PoliklinikTarihi?.ToString("yyyy-MM-dd"),
            x.VakaKodu,
            x.VakaAdi
        ));

        return Ok(result);
    }
}
