using Microsoft.AspNetCore.Mvc;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using SgkVizite;
using System.Collections.Generic;

namespace RaporServisi.Api.Controllers
{
    [ApiController]
    [Route("api/v1/sgk/reports")]
    public class SgkReportsController : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest dto)
        {
            var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport)
            {
                MaxReceivedMessageSize = 10 * 1024 * 1024,
                OpenTimeout = TimeSpan.FromSeconds(60),
                SendTimeout = TimeSpan.FromSeconds(60),
                ReceiveTimeout = TimeSpan.FromSeconds(60)
            };

            var endpoint = new EndpointAddress("https://uyg.sgk.gov.tr/Ws_Vizite/services/ViziteGonder");
            var client = new ViziteGonderClient(binding, endpoint);

            var loginResp = await client.wsLoginAsync(dto.KullaniciAdi, dto.IsyeriKodu, dto.WsSifre);
            var result = loginResp?.wsLoginReturn;

            if (result == null || result.sonucKod != 0 || string.IsNullOrWhiteSpace(result.wsToken))
            {
                return StatusCode(502, new
                {
                    success = false,
                    sonucKod = result?.sonucKod ?? -1,
                    sonucAciklama = result?.sonucAciklama ?? "Bilinmeyen hata"
                });
            }

            return Ok(new
            {
                success = true,
                sonucKod = result.sonucKod,
                sonucAciklama = result.sonucAciklama,
                wsToken = result.wsToken
            });
        }

        [HttpPost("search-by-date")]
        public async Task<IActionResult> SearchByDate([FromBody] ReportSearchRequest dto)
        {
            if (!DateTime.TryParse(dto.Tarih, out var date))
                return BadRequest("Tarih formatı geçersiz. Format: dd.MM.yyyy");

            var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport)
            {
                MaxReceivedMessageSize = 10 * 1024 * 1024,
                OpenTimeout = TimeSpan.FromSeconds(60),
                SendTimeout = TimeSpan.FromSeconds(60),
                ReceiveTimeout = TimeSpan.FromSeconds(60)
            };

            var endpoint = new EndpointAddress("https://uyg.sgk.gov.tr/Ws_Vizite/services/ViziteGonder");
            var client = new ViziteGonderClient(binding, endpoint);

            var resp = await client.raporAramaTarihileAsync(
                dto.KullaniciAdi,
                dto.IsyeriKodu,
                dto.WsToken,
                date.ToString("dd.MM.yyyy")
            );

            var result = resp?.raporAramaTarihileReturn;

            if (result == null || result.sonucKod != 0)
            {
                return Ok(new
                {
                    success = false,
                    sonucKod = result?.sonucKod ?? -1,
                    sonucAciklama = result?.sonucAciklama ?? "Bilinmeyen hata"
                });
            }

            var items = new List<object>();

            if (result.raporAramaTarihleBeanArray != null)
            {
                foreach (var r in result.raporAramaTarihleBeanArray)
                {
                    items.Add(new
                    {
                        TcKimlikNo = r?.TCKIMLIKNO,
                        Ad = r?.AD?.Trim(),
                        Soyad = r?.SOYAD?.Trim(),
                        MedulaRaporId = r?.MEDULARAPORID,
                        RaporTakipNo = r?.RAPORTAKIPNO,
                        PoliklinikTarihi = r?.POLIKLINIKTAR,
                        RaporDurumu = r?.RAPORDURUMU,
                        Vaka = r?.VAKA,
                        VakaAdi = r?.VAKAADI,
                        TesisAdi = r?.TESISADI
                    });
                }
            }

            return Ok(new
            {
                success = true,
                sonucKod = result.sonucKod,
                sonucAciklama = result.sonucAciklama,
                date = date.ToString("yyyy-MM-dd"),
                raporlar = items
            });
        }

        [HttpPost("search-by-tc")]
        public async Task<IActionResult> SearchByTc([FromBody] ReportSearchByTcRequest dto)
        {
            var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport)
            {
                MaxReceivedMessageSize = 10 * 1024 * 1024,
                OpenTimeout = TimeSpan.FromSeconds(60),
                SendTimeout = TimeSpan.FromSeconds(60),
                ReceiveTimeout = TimeSpan.FromSeconds(60)
            };

            var endpoint = new EndpointAddress("https://uyg.sgk.gov.tr/Ws_Vizite/services/ViziteGonder");
            var client = new ViziteGonderClient(binding, endpoint);

            var resp = await client.raporAramaKimlikNoAsync(
                dto.KullaniciAdi,
                dto.IsyeriKodu,
                dto.WsToken,
                dto.TcKimlikNo
            );

            var result = resp?.raporAramaKimlikNoReturn;

            if (result == null || result.sonucKod != 0)
            {
                return Ok(new
                {
                    success = false,
                    sonucKod = result?.sonucKod ?? -1,
                    sonucAciklama = result?.sonucAciklama ?? "Bilinmeyen hata"
                });
            }

            var items = new List<object>();

            if (result.raporBeanArray != null)
            {
                foreach (var r in result.raporBeanArray)
                {
                    items.Add(new
                    {
                        TcKimlikNo = r?.TCKIMLIKNO,
                        MedulaRaporId = r?.MEDULARAPORID,
                        RaporDurumu = r?.RAPORDURUMU,
                        Vaka = r?.VAKA,
                        VakaAdi = r?.VAKAADI,
                        PoliklinikTarihi = r?.POLIKLINIKTAR,
                        RaporBitTar = r?.RAPORBITTAR,
                        TesisAdi = r?.TESISADI
                    });
                }
            }

            return Ok(new
            {
                success = true,
                sonucKod = result.sonucKod,
                sonucAciklama = result.sonucAciklama,
                raporlar = items
            });
        }
    }

    public class LoginRequest
    {
        public string KullaniciAdi { get; set; } = "";
        public string IsyeriKodu { get; set; } = "";
        public string WsSifre { get; set; } = "";
    }

    public class ReportSearchRequest
    {
        public string KullaniciAdi { get; set; } = "";
        public string IsyeriKodu { get; set; } = "";
        public string WsToken { get; set; } = "";
        public string Tarih { get; set; } = "";
    }

    public class ReportSearchByTcRequest
    {
        public string KullaniciAdi { get; set; } = "";
        public string IsyeriKodu { get; set; } = "";
        public string WsToken { get; set; } = "";
        public string TcKimlikNo { get; set; } = "";
    }
}
