using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Http.Headers;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using RaporServisi.Application.Contracts;

namespace RaporServisi.Infrastructure.External;

public class SgkViziteClient : ISgkViziteClient
{
    private readonly HttpClient _http;
    private readonly SgkViziteOptions _opt;
    private string? _tokenCache;
    private DateTime _tokenTimeUtc;

    public SgkViziteClient(HttpClient http, IOptions<SgkViziteOptions> opt)
    {
        _http = http;
        _opt = opt.Value;
    }

    public async Task<string> LoginAsync(CancellationToken ct)
    {
        if (_tokenCache is not null && DateTime.UtcNow - _tokenTimeUtc < TimeSpan.FromMinutes(25))
            return _tokenCache;

        var body = $@"
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:viz=""http://vizite.ws.sgk"">
  <soapenv:Header/>
  <soapenv:Body>
    <viz:wsLogin>
      <viz:KullaniciAdi>{_opt.KullaniciAdi}</viz:KullaniciAdi>
      <viz:IsyeriKodu>{_opt.IsyeriKodu}</viz:IsyeriKodu>
      <viz:WsSifre>{_opt.WsSifre}</viz:WsSifre>
    </viz:wsLogin>
  </soapenv:Body>
</soapenv:Envelope>";

        var req = new StringContent(body);
        req.Headers.ContentType = new MediaTypeHeaderValue("text/xml");


        var resp = await _http.PostAsync(_opt.Endpoint, req, ct);
        resp.EnsureSuccessStatusCode();

        var xml = XDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        var token = xml.Descendants().FirstOrDefault(x => x.Name.LocalName is "wsToken" or "token")?.Value;
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("WS Login başarısız: token bulunamadı.");

        _tokenCache = token;
        _tokenTimeUtc = DateTime.UtcNow;
        return token;
    }

    public async Task<IReadOnlyList<ViziteReportSummary>> GetReportsByDateAsync(DateTime tarih, CancellationToken ct)
    {
        var token = await LoginAsync(ct);
        var tarihStr = tarih.ToString("dd.MM.yyyy");

        var body = $@"
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:viz=""http://vizite.ws.sgk"">
  <soapenv:Header/>
  <soapenv:Body>
    <viz:RaporAramaTarihile>
      <viz:WsToken>{token}</viz:WsToken>
      <viz:KullaniciAdi>{_opt.KullaniciAdi}</viz:KullaniciAdi>
      <viz:IsyeriKodu>{_opt.IsyeriKodu}</viz:IsyeriKodu>
      <viz:Tarih>{tarihStr}</viz:Tarih>
    </viz:RaporAramaTarihile>
  </soapenv:Body>
</soapenv:Envelope>";

        var req = new StringContent(body);
        req.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
        var resp = await _http.PostAsync(_opt.Endpoint, req, ct);
        resp.EnsureSuccessStatusCode();

        var xml = XDocument.Parse(await resp.Content.ReadAsStringAsync(ct));

        // Dönen alan adları WSDL’e göre isim/namespace farkı gösterebilir; LocalName ile topla.
        var items = xml.Descendants().Where(d => d.Name.LocalName is "item" or "rapor")
            .Select(n =>
            {
                long.TryParse(n.Descendants().FirstOrDefault(x => x.Name.LocalName == "MEDULARAPORID")?.Value ??
                               n.Descendants().FirstOrDefault(x => x.Name.LocalName == "MedulaRaporId")?.Value, out var id);

                var tckn = n.Descendants().FirstOrDefault(x => x.Name.LocalName is "TCKIMLIKNO" or "TcKimlikNo" or "TCKN")?.Value ?? "";

                DateTime? poli = null;
                var poliRaw = n.Descendants().FirstOrDefault(x => x.Name.LocalName is "POLIKLINIKTAR" or "PoliklinikTarihi")?.Value;
                if (DateTime.TryParse(poliRaw, out var pdt)) poli = pdt;

                int? vaka = null;
                var vakaRaw = n.Descendants().FirstOrDefault(x => x.Name.LocalName is "VAKA" or "Vaka")?.Value;
                if (int.TryParse(vakaRaw, out var v)) vaka = v;

                var vakaAdi = n.Descendants().FirstOrDefault(x => x.Name.LocalName is "VAKAADI" or "VakaAdi")?.Value;

                return new ViziteReportSummary(id, tckn, poli, vaka, vakaAdi);
            })
            .ToList();

        return items;
    }

    public async Task<bool> MarkReportAsReadAsync(long medulaRaporId, CancellationToken ct)
    {
        var token = await LoginAsync(ct);
        var body = $@"
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:viz=""http://vizite.ws.sgk"">
  <soapenv:Header/>
  <soapenv:Body>
    <viz:RaporOkunduKapat>
      <viz:WsToken>{token}</viz:WsToken>
      <viz:KullaniciAdi>{_opt.KullaniciAdi}</viz:KullaniciAdi>
      <viz:IsyeriKodu>{_opt.IsyeriKodu}</viz:IsyeriKodu>
      <viz:MedulaRaporId>{medulaRaporId}</viz:MedulaRaporId>
    </viz:RaporOkunduKapat>
  </soapenv:Body>
</soapenv:Envelope>";

        var req = new StringContent(body);
        req.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
        var resp = await _http.PostAsync(_opt.Endpoint, req, ct);
        resp.EnsureSuccessStatusCode();

        var xml = XDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        // Başarı göstergesi alanı WSDL’e göre değişebilir; true/OK/SONUC vb.
        var sonuc = xml.Descendants().FirstOrDefault(x => x.Name.LocalName is "Sonuc" or "result" or "ok")?.Value;
        return string.IsNullOrWhiteSpace(sonuc) || sonuc.Equals("true", StringComparison.OrdinalIgnoreCase) || sonuc.Equals("OK", StringComparison.OrdinalIgnoreCase);
    }
}

