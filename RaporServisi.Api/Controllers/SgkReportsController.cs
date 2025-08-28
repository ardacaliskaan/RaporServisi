using Microsoft.AspNetCore.Mvc;
using SgkVizite;
using System.ServiceModel;
using System.ServiceModel.Channels;
namespace RaporServisi.Api.Controllers;

public record SgkReportQueryDto(string KullaniciAdi, string IsyeriKodu, string WsSifre, string Tarih);
public record SgkReportListItem(
    string? TcKimlikNo, string? MedulaRaporId, string? RaporDurumu,
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

        // Client
        var binding = new BasicHttpBinding(BasicHttpSecurityMode.None)
        {
            MaxReceivedMessageSize = 10 * 1024 * 1024,
            OpenTimeout = TimeSpan.FromSeconds(60),
            SendTimeout = TimeSpan.FromSeconds(60),
            ReceiveTimeout = TimeSpan.FromSeconds(60)
        };

        // WSDL’deki adres (HTTP/9094)
        var endpoint = new EndpointAddress("http://uyg.sgk.gov.tr:9094/Ws_Vizite/services/ViziteGonder");

        // Generated client bu ctor’u destekler
        var client = new ViziteGonderClient(binding, endpoint);

        // Login
        var loginResp = await client.wsLoginAsync(
            dto.KullaniciAdi,   // kullaniciAdi 
            dto.IsyeriKodu,     // isyeriKodu    
            dto.WsSifre         // isyeriSifresi 
        );
        var token = loginResp.wsLoginReturn?.wsToken;

        var loginKod = loginResp?.wsLoginReturn?.sonucKod ?? -1;
        if (string.IsNullOrWhiteSpace(token) || loginKod != 0)
            return StatusCode(502, $"WS Login başarısız: kod={loginKod}, açıklama='{loginResp?.wsLoginReturn?.sonucAciklama}'.");

        // 2) Raporları tarihle ara
        var tarihleResp = await client.raporAramaTarihileAsync(
            dto.KullaniciAdi,
            dto.IsyeriKodu,
            token!,
            dt.ToString("dd.MM.yyyy")
        );                                                                                        

        var cevap = tarihleResp?.raporAramaTarihileReturn; // tip CevapRapor
        var items = new List<SgkReportListItem>();

        // Hata kodu kontrolü
        var kod = cevap?.sonucKod ?? -1;
        if (kod != 0)
        {
            return Ok(new
            {
                success = false,
                sonucKod = kod,
                sonucAciklama = cevap?.sonucAciklama,
                date = dt.ToString("yyyy-MM-dd"),
                items
            });
        }

        // CevapRapor içindeki iki olası koleksiyondan okumalar
        if (cevap?.tarihSorguBean != null) // TarihSorguBean[]                                      
        {
            foreach (var r in cevap.tarihSorguBean)
            {
                items.Add(new SgkReportListItem(
                    TcKimlikNo: r?.TCKIMLIKNO,
                    MedulaRaporId: r?.MEDULARAPORID,
                    RaporDurumu: r?.RAPORDURUMU ?? r?.RAPORDURUMADI,
                    Vaka: r?.VAKA,
                    VakaAdi: r?.VAKAADI,
                    PoliklinikTarihi: r?.POLIKLINIKTAR,
                    RaporBitTar: r?.RAPORBITTAR,
                    TesisAdi: r?.TESISADI
                ));
            }
        }

        if (cevap?.raporBeanArray != null) // RaporBean[]                                          
        {
            foreach (var r in cevap.raporBeanArray)
            {
                items.Add(new SgkReportListItem(
                    TcKimlikNo: r?.TCKIMLIKNO,
                    MedulaRaporId: r?.MEDULARAPORID,
                    RaporDurumu: r?.RAPORDURUMU,
                    Vaka: r?.VAKA,
                    VakaAdi: r?.VAKAADI,
                    PoliklinikTarihi: r?.POLIKLINIKTAR,
                    RaporBitTar: r?.RAPORBITTAR,
                    TesisAdi: r?.TESISADI
                ));
            }
        }

        return Ok(new
        {
            success = true,
            count = items.Count,
            date = dt.ToString("yyyy-MM-dd"),
            items
        });
    }
}
