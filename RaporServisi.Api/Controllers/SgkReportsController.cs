using Microsoft.AspNetCore.Mvc;
using SgkVizite;

namespace RaporServisi.Api.Controllers;

public record SgkReportQueryDto(string KullaniciAdi, string IsyeriKodu, string WsSifre, string Tarih);
public record SgkReportListItem(string? TcKimlikNo, string? MedulaRaporId, string? RaporDurumu,
                                string? Vaka, string? VakaAdi, string? PoliklinikTarihi,
                                string? RaporBitTar, string? TesisAdi);

[ApiController]
[Route("api/v1/sgk/reports")]
public class SgkReportsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> List([FromBody] SgkReportQueryDto dto)
    {
        if (!DateTime.TryParse(dto.Tarih, out var dt))
            return BadRequest("Tarih formatı geçersiz (örn: 28.08.2025 veya 2025-08-28).");

        var client = new ViziteGonderClient(ViziteGonderClient.EndpointConfiguration.ViziteGonder);

        // 1) Login — 3 string overload (POZİSYONLA)
        var loginResp = await client.wsLoginAsync(dto.KullaniciAdi, dto.IsyeriKodu, dto.WsSifre);
        var token = loginResp.wsLoginReturn?.wsToken;
        if (string.IsNullOrWhiteSpace(token))
            return StatusCode(502, "WS Login başarısız: token alınamadı.");

        // 2) Rapor arama — 4 string overload (POZİSYONLA)
        var raporResp = await client.raporAramaTarihileAsync(
            dto.KullaniciAdi,
            dto.IsyeriKodu,
            token!,
            dt.ToString("dd.MM.yyyy")
        );

        var cevap = raporResp.raporAramaTarihileReturn; // CevapRapor
        var items = new List<SgkReportListItem>();

        // TarihSorguBean[]
        if (cevap?.tarihSorguBean != null)
        {
            foreach (var r in cevap.tarihSorguBean)
            {
                items.Add(new SgkReportListItem(
                    r?.TCKIMLIKNO, r?.MEDULARAPORID, r?.RAPORDURUMU ?? r?.RAPORDURUMADI,
                    r?.VAKA, r?.VAKAADI, r?.POLIKLINIKTAR, r?.RAPORBITTAR, r?.TESISADI
                ));
            }
        }

        // RaporBean[]
        if (cevap?.raporBeanArray != null)
        {
            foreach (var r in cevap.raporBeanArray)
            {
                items.Add(new SgkReportListItem(
                    r?.TCKIMLIKNO, r?.MEDULARAPORID, r?.RAPORDURUMU,
                    r?.VAKA, r?.VAKAADI, r?.POLIKLINIKTAR, r?.RAPORBITTAR, r?.TESISADI
                ));
            }
        }

        return Ok(new { count = items.Count, date = dt.ToString("yyyy-MM-dd"), items });
    }
}
