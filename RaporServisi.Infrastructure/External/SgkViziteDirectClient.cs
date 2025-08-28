using System.Net.Http.Headers;
using System.Xml.Linq;

namespace RaporServisi.Infrastructure.External;

public record ViziteLogin(string KullaniciAdi, string IsyeriKodu, string WsSifre);
public record ViziteReportItem(long MedulaRaporId, string Tckn, DateTime? PoliklinikTarihi, int? VakaKodu, string? VakaAdi);

public class SgkViziteDirectClient
{
    private readonly HttpClient _http;
    private const string Endpoint = "https://uyg.sgk.gov.tr/Ws_Vizite/services/ViziteGonder";

    public SgkViziteDirectClient(HttpClient http) => _http = http;

    public async Task<string> LoginAsync(ViziteLogin cred, CancellationToken ct)
    {
        var body = $@"
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:viz=""http://vizite.ws.sgk"">
  <soapenv:Header/>
  <soapenv:Body>
    <viz:wsLogin>
      <viz:KullaniciAdi>{cred.KullaniciAdi}</viz:KullaniciAdi>
      <viz:IsyeriKodu>{cred.IsyeriKodu}</viz:IsyeriKodu>
      <viz:WsSifre>{cred.WsSifre}</viz:WsSifre>
    </viz:wsLogin>
  </soapenv:Body>
</soapenv:Envelope>";
        var req = new StringContent(body);
        req.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
        var resp = await _http.PostAsync(Endpoint, req, ct);
        resp.EnsureSuccessStatusCode();

        var xml = XDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
        var token = xml.Descendants().FirstOrDefault(x => x.Name.LocalName is "wsToken" or "token")?.Value;
        if (string.IsNullOrWhiteSpace(token)) throw new InvalidOperationException("WS Login başarısız (token yok).");
        return token!;
    }

    public async Task<IReadOnlyList<ViziteReportItem>> GetReportsByDateAsync(
        ViziteLogin cred, DateTime tarih, CancellationToken ct)
    {
        var token = await LoginAsync(cred, ct);
        var tarihStr = tarih.ToString("dd.MM.yyyy");

        var body = $@"
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:viz=""http://vizite.ws.sgk"">
  <soapenv:Header/>
  <soapenv:Body>
    <viz:RaporAramaTarihile>
      <viz:WsToken>{token}</viz:WsToken>
      <viz:KullaniciAdi>{cred.KullaniciAdi}</viz:KullaniciAdi>
      <viz:IsyeriKodu>{cred.IsyeriKodu}</viz:IsyeriKodu>
      <viz:Tarih>{tarihStr}</viz:Tarih>
    </viz:RaporAramaTarihile>
  </soapenv:Body>
</soapenv:Envelope>";

        var req = new StringContent(body);
        req.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
        var resp = await _http.PostAsync(Endpoint, req, ct);
        resp.EnsureSuccessStatusCode();

        var xml = XDocument.Parse(await resp.Content.ReadAsStringAsync(ct));

        var items = xml.Descendants().Where(n => n.Name.LocalName is "item" or "rapor")
            .Select(n =>
            {
                long.TryParse(n.Descendants().FirstOrDefault(x => x.Name.LocalName is "MEDULARAPORID" or "MedulaRaporId")?.Value, out var id);
                var tckn = n.Descendants().FirstOrDefault(x => x.Name.LocalName is "TCKIMLIKNO" or "TcKimlikNo" or "TCKN")?.Value ?? "";
                DateTime? poli = null;
                var poliRaw = n.Descendants().FirstOrDefault(x => x.Name.LocalName is "POLIKLINIKTAR" or "PoliklinikTarihi")?.Value;
                if (DateTime.TryParse(poliRaw, out var pdt)) poli = pdt;
                int? vaka = null;
                var vakaRaw = n.Descendants().FirstOrDefault(x => x.Name.LocalName is "VAKA" or "Vaka")?.Value;
                if (int.TryParse(vakaRaw, out var v)) vaka = v;
                var vakaAdi = n.Descendants().FirstOrDefault(x => x.Name.LocalName is "VAKAADI" or "VakaAdi")?.Value;
                return new ViziteReportItem(id, tckn, poli, vaka, vakaAdi);
            })
            .ToList();

        return items;
    }
}
