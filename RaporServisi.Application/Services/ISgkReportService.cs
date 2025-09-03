using RaporServisi.Application.DTOs;

namespace RaporServisi.Application.Services;

public interface ISgkReportService
{
    /// <summary>
    /// SGK WS Login - 30 dakikalık token alır
    /// </summary>
    /// <param name="request">Login bilgileri</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Token bilgisi</returns>
    Task<ApiResponseDto<string>> LoginAsync(SgkLoginRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Tarih ile rapor arama - Güncel rapor listesi (METOT 2)
    /// Dokümana göre: Poliklinik tarihi girilen tarihten küçük olan ilk 100 rapor
    /// Rate limit: Aynı işveren için 24 saatte en fazla 2 sorgu
    /// </summary>
    /// <param name="request">Rapor arama parametreleri</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Rapor listesi</returns>
    Task<ApiResponseDto<RaporAramaResponseDto>> RaporAramaTarihileAsync(RaporAramaRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Onaylı raporlar listesi - Geçmiş onaylanmış raporlar (METOT 8)
    /// </summary>
    /// <param name="request">Tarih aralığı parametreleri</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Onaylı rapor listesi</returns>
    Task<ApiResponseDto<OnayliRaporlarResponseDto>> OnayliRaporlarTarihileAsync(OnayliRaporlarRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Raporu okundu olarak işaretle ve kapat (METOT 9)
    /// Dokümana göre: Raporu başarılı okundu, sonraki sorguya dahil olmasın
    /// ÖNEMLİ: Bu metot çağrılmazsa diğer raporlara erişim engellenir
    /// </summary>
    /// <param name="request">Rapor kapatma parametreleri</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>İşlem sonucu</returns>
    Task<ApiResponseDto<SgkOperationResultDto>> RaporOkunduKapatAsync(RaporOkunduKapatRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Rapor onaylama - Çalışmazlık beyanı alma (METOT 4)
    /// </summary>
    /// <param name="request">Rapor onay parametreleri</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Onay işlem sonucu</returns>
    Task<ApiResponseDto<SgkOperationResultDto>> RaporOnayAsync(RaporOnayRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Rate limiting kontrolü - İşveren için istek yapılabilir mi kontrol eder
    /// </summary>
    /// <param name="isyeriKodu">İşyeri kodu</param>
    /// <param name="metodAdi">Hangi metot için kontrol (RaporAramaTarihile, HasIsKazSorguTarihle)</param>
    /// <returns>Rate limit durumu</returns>
    Task<RateLimitInfoDto> CheckRateLimitAsync(string isyeriKodu, string metodAdi);

    /// <summary>
    /// Toplu işlem - Bir istekte hem güncel hem geçmiş raporları getirir
    /// </summary>
    /// <param name="request">Rapor arama parametreleri</param>
    /// <param name="includeHistorical">5 yıl öncesi onaylı raporları da getir</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Birleştirilmiş rapor listesi</returns>
    Task<ApiResponseDto<ComprehensiveReportDto>> GetComprehensiveReportsAsync(
        RaporAramaRequestDto request,
        bool includeHistorical = true,
        CancellationToken ct = default);
}

// Comprehensive için ek DTO
public class ComprehensiveReportDto : SgkBaseResponseDto
{
    public RaporAramaResponseDto GuncelRaporlar { get; set; } = new();
    public OnayliRaporlarResponseDto GecmisRaporlar { get; set; } = new();
    public int ToplamIslem { get; set; }
    public List<string> Uyarilar { get; set; } = new();
    public bool RateLimitAsildi { get; set; }
}