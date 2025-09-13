using Microsoft.Extensions.Logging;
using RaporServisi.Application.DTOs;
using RaporServisi.Application.Services;
using SgkVizite;

namespace RaporServisi.Infrastructure.Services;

public class SgkReportService : ISgkReportService
{
    private readonly ViziteGonderClient _soapClient;
    private readonly ILogger<SgkReportService> _logger;

    public SgkReportService(
        ViziteGonderClient soapClient,
        ILogger<SgkReportService> logger)
    {
        _soapClient = soapClient;
        _logger = logger;
    }

    public async Task<ApiResponseDto<string>> LoginAsync(SgkLoginRequestDto request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("SGK Login işlemi başlatılıyor - İşyeri: {IsyeriKodu}", request.IsyeriKodu);

            var result = await _soapClient.wsLoginAsync(request.KullaniciAdi, request.IsyeriKodu, request.WsSifre);

            if (result?.wsLoginReturn?.sonucKod == 0 && !string.IsNullOrWhiteSpace(result.wsLoginReturn.wsToken))
            {
                _logger.LogInformation("SGK Login başarılı - İşyeri: {IsyeriKodu}", request.IsyeriKodu);

                return new ApiResponseDto<string>
                {
                    Success = true,
                    Message = "Login başarılı. Token 30 dakika geçerlidir.",
                    Data = result.wsLoginReturn.wsToken
                };
            }

            _logger.LogWarning("SGK Login başarısız - İşyeri: {IsyeriKodu}, SonucKod: {SonucKod}, Mesaj: {Mesaj}",
                request.IsyeriKodu, result?.wsLoginReturn?.sonucKod, result?.wsLoginReturn?.sonucAciklama);

            return new ApiResponseDto<string>
            {
                Success = false,
                Message = result?.wsLoginReturn?.sonucAciklama ?? "Login işlemi başarısız",
                Errors = new List<string> { $"Hata Kodu: {result?.wsLoginReturn?.sonucKod ?? -1}" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SGK Login işleminde hata - İşyeri: {IsyeriKodu}", request.IsyeriKodu);

            return new ApiResponseDto<string>
            {
                Success = false,
                Message = "Login işleminde sistem hatası",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponseDto<RaporAramaResponseDto>> RaporAramaTarihileAsync(RaporAramaRequestDto request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("RaporAramaTarihile işlemi başlatılıyor - İşyeri: {IsyeriKodu}, Tarih: {Tarih}",
                request.IsyeriKodu, request.Tarih);

            // Login
            var loginResult = await LoginAsync(request, ct);
            if (!loginResult.Success || string.IsNullOrWhiteSpace(loginResult.Data))
            {
                return new ApiResponseDto<RaporAramaResponseDto>
                {
                    Success = false,
                    Message = "Login başarısız: " + loginResult.Message,
                    Errors = loginResult.Errors
                };
            }

            var token = loginResult.Data;

            // RaporAramaTarihile çağrısı
            var result = await _soapClient.raporAramaTarihileAsync(
                request.KullaniciAdi, request.IsyeriKodu, token, request.Tarih);
            if (result?.raporAramaTarihileReturn?.sonucKod == 0)
            {
                var raporlar = MapToRaporItemDtoList(result.raporAramaTarihileReturn.raporAramaTarihleBeanArray);

                var responseDto = new RaporAramaResponseDto
                {
                    Success = true,
                    Message = $"{raporlar.Count} rapor bulundu",
                    Raporlar = raporlar,
                    ToplamRapor = raporlar.Count,
                    SorguTarihi = request.Tarih
                };

                _logger.LogInformation("RaporAramaTarihile başarılı - İşyeri: {IsyeriKodu}, Bulunan: {Count}",
                    request.IsyeriKodu, raporlar.Count);

                return new ApiResponseDto<RaporAramaResponseDto>
                {
                    Success = true,
                    Message = "Rapor arama işlemi başarılı",
                    Data = responseDto
                };
            }

            var errorMessage = result?.raporAramaTarihileReturn?.sonucAciklama ?? "Bilinmeyen hata";
            _logger.LogWarning("RaporAramaTarihile başarısız - İşyeri: {IsyeriKodu}, Hata: {Hata}",
                request.IsyeriKodu, errorMessage);

            return new ApiResponseDto<RaporAramaResponseDto>
            {
                Success = false,
                Message = errorMessage,
                Errors = new List<string> { $"SGK Hata Kodu: {result?.raporAramaTarihileReturn?.sonucKod}" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RaporAramaTarihile işleminde hata - İşyeri: {IsyeriKodu}", request.IsyeriKodu);

            return new ApiResponseDto<RaporAramaResponseDto>
            {
                Success = false,
                Message = "Rapor arama işleminde sistem hatası",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponseDto<OnayliRaporlarResponseDto>> OnayliRaporlarTarihileAsync(OnayliRaporlarRequestDto request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("OnayliRaporlarTarihile işlemi başlatılıyor - İşyeri: {IsyeriKodu}", request.IsyeriKodu);

            // Login
            var loginResult = await LoginAsync(request, ct);
            if (!loginResult.Success || string.IsNullOrWhiteSpace(loginResult.Data))
            {
                return new ApiResponseDto<OnayliRaporlarResponseDto>
                {
                    Success = false,
                    Message = "Login başarısız: " + loginResult.Message,
                    Errors = loginResult.Errors
                };
            }

            var token = loginResult.Data;

            // OnayliRaporlarTarihile çağrısı
            var result = await _soapClient.onayliRaporlarTarihileAsync(
                request.KullaniciAdi, request.IsyeriKodu, token, request.BaslangicTarihi, request.BitisTarihi);

            if (result?.onayliRaporlarTarihileReturn?.sonucKod == 0)
            {
                var raporlar = MapToOnayliRaporItemDtoList(result.onayliRaporlarTarihileReturn.onayliRaporlarTarihleBeanArray);

                var responseDto = new OnayliRaporlarResponseDto
                {
                    Success = true,
                    Message = $"{raporlar.Count} onaylı rapor bulundu",
                    Raporlar = raporlar,
                    ToplamRapor = raporlar.Count,
                    BaslangicTarihi = request.BaslangicTarihi,
                    BitisTarihi = request.BitisTarihi
                };

                return new ApiResponseDto<OnayliRaporlarResponseDto>
                {
                    Success = true,
                    Message = "Onaylı rapor sorgusu başarılı",
                    Data = responseDto
                };
            }

            return new ApiResponseDto<OnayliRaporlarResponseDto>
            {
                Success = false,
                Message = result?.onayliRaporlarTarihileReturn?.sonucAciklama ?? "Bilinmeyen hata",
                Errors = new List<string> { $"SGK Hata Kodu: {result?.onayliRaporlarTarihileReturn?.sonucKod}" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OnayliRaporlarTarihile işleminde hata - İşyeri: {IsyeriKodu}", request.IsyeriKodu);

            return new ApiResponseDto<OnayliRaporlarResponseDto>
            {
                Success = false,
                Message = "Onaylı rapor sorgusu başarısız",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponseDto<SgkOperationResultDto>> RaporOkunduKapatAsync(RaporOkunduKapatRequestDto request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("RaporOkunduKapat işlemi - İşyeri: {IsyeriKodu}, RaporId: {RaporId}",
                request.IsyeriKodu, request.MedulaRaporId);

            // Login
            var loginResult = await LoginAsync(request, ct);
            if (!loginResult.Success || string.IsNullOrWhiteSpace(loginResult.Data))
            {
                return new ApiResponseDto<SgkOperationResultDto>
                {
                    Success = false,
                    Message = "Login başarısız: " + loginResult.Message,
                    Errors = loginResult.Errors
                };
            }

            var token = loginResult.Data;

            // RaporOkunduKapat çağrısı
            var result = await _soapClient.raporOkunduKapatAsync(
                request.KullaniciAdi, request.IsyeriKodu, token, request.MedulaRaporId.ToString());

            var operationResult = new SgkOperationResultDto
            {
                Success = result?.raporOkunduKapatReturn?.sonucKod == 0,
                SonucKod = result?.raporOkunduKapatReturn?.sonucKod ?? -1,
                SonucAciklama = result?.raporOkunduKapatReturn?.sonucAciklama ?? "Bilinmeyen hata"
            };

            return new ApiResponseDto<SgkOperationResultDto>
            {
                Success = operationResult.Success,
                Message = operationResult.Success ? "Rapor başarıyla kapatıldı" : operationResult.SonucAciklama,
                Data = operationResult
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RaporOkunduKapat işleminde hata - İşyeri: {IsyeriKodu}", request.IsyeriKodu);

            return new ApiResponseDto<SgkOperationResultDto>
            {
                Success = false,
                Message = "Rapor kapatma işleminde hata",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponseDto<SgkOperationResultDto>> RaporOnayAsync(RaporOnayRequestDto request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("RaporOnay işlemi - İşyeri: {IsyeriKodu}, RaporId: {RaporId}",
                request.IsyeriKodu, request.MedulaRaporId);

            // Login
            var loginResult = await LoginAsync(request, ct);
            if (!loginResult.Success || string.IsNullOrWhiteSpace(loginResult.Data))
            {
                return new ApiResponseDto<SgkOperationResultDto>
                {
                    Success = false,
                    Message = "Login başarısız: " + loginResult.Message,
                    Errors = loginResult.Errors
                };
            }

            var token = loginResult.Data;

            // RaporOnay çağrısı
            var result = await _soapClient.raporOnayAsync(
                request.KullaniciAdi, request.IsyeriKodu, token, request.TcKimlikNo,
                request.Vaka, request.MedulaRaporId.ToString(), request.NitelikDurumu, request.Tarih);

            var operationResult = new SgkOperationResultDto
            {
                Success = result?.raporOnayReturn?.sonucKod == 0,
                SonucKod = result?.raporOnayReturn?.sonucKod ?? -1,
                SonucAciklama = result?.raporOnayReturn?.sonucAciklama ?? "Bilinmeyen hata"
            };

            return new ApiResponseDto<SgkOperationResultDto>
            {
                Success = operationResult.Success,
                Message = operationResult.Success ? "Rapor başarıyla onaylandı" : operationResult.SonucAciklama,
                Data = operationResult
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RaporOnay işleminde hata - İşyeri: {IsyeriKodu}", request.IsyeriKodu);

            return new ApiResponseDto<SgkOperationResultDto>
            {
                Success = false,
                Message = "Rapor onay işleminde hata",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<RateLimitInfoDto> CheckRateLimitAsync(string isyeriKodu, string metodAdi)
    {
        return await Task.FromResult(new RateLimitInfoDto
        {
            CanMakeRequest = true,
            RemainingRequests = int.MaxValue,
            Message = "Rate limiting devre dışı - Sınırsız istek yapılabilir"
        });
    }

    public async Task<ApiResponseDto<ComprehensiveReportDto>> GetComprehensiveReportsAsync(
        RaporAramaRequestDto request, bool includeHistorical = true, CancellationToken ct = default)
    {
        try
        {
            var comprehensiveResult = new ComprehensiveReportDto();
            var uyarilar = new List<string>();

            // Güncel raporları getir
            var guncelResult = await RaporAramaTarihileAsync(request, ct);
            if (guncelResult.Success && guncelResult.Data != null)
            {
                comprehensiveResult.GuncelRaporlar = guncelResult.Data;
            }
            else
            {
                uyarilar.Add($"Güncel rapor sorgusu başarısız: {guncelResult.Message}");
            }

            // 5 yıl öncesi onaylı raporları getir
            if (includeHistorical && DateTime.TryParse(request.Tarih, out var date))
            {
                var historicalRequest = new OnayliRaporlarRequestDto
                {
                    KullaniciAdi = request.KullaniciAdi,
                    IsyeriKodu = request.IsyeriKodu,
                    WsSifre = request.WsSifre,
                    BaslangicTarihi = date.AddYears(-5).ToString("dd.MM.yyyy"),
                    BitisTarihi = request.Tarih
                };

                var gecmisResult = await OnayliRaporlarTarihileAsync(historicalRequest, ct);
                if (gecmisResult.Success && gecmisResult.Data != null)
                {
                    comprehensiveResult.GecmisRaporlar = gecmisResult.Data;
                }
                else
                {
                    uyarilar.Add($"Geçmiş rapor sorgusu başarısız: {gecmisResult.Message}");
                }
            }

            comprehensiveResult.Success = true;
            comprehensiveResult.Uyarilar = uyarilar;
            comprehensiveResult.ToplamIslem = comprehensiveResult.GuncelRaporlar.ToplamRapor +
                                           comprehensiveResult.GecmisRaporlar.ToplamRapor;
            comprehensiveResult.RateLimitAsildi = false; // Artık asılmayacak

            return new ApiResponseDto<ComprehensiveReportDto>
            {
                Success = true,
                Message = $"Toplam {comprehensiveResult.ToplamIslem} rapor işlendi",
                Data = comprehensiveResult
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetComprehensiveReports işleminde hata");

            return new ApiResponseDto<ComprehensiveReportDto>
            {
                Success = false,
                Message = "Kapsamlı rapor sorgusu başarısız",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    // Mapping metodları
    private List<RaporItemDto> MapToRaporItemDtoList(RaporAramaTarihleBean[]? beans)
    {
        if (beans == null) return new List<RaporItemDto>();

        return beans.Select(b => new RaporItemDto
        {
            TcKimlikNo = b.TCKIMLIKNO ?? "",
            Ad = b.AD ?? "",
            Soyad = b.SOYAD ?? "",
            MedulaRaporId = long.TryParse(b.MEDULARAPORID, out var id) ? id : 0,
            RaporTakipNo = b.RAPORTAKIPNO ?? "",
            RaporSiraNo = b.RAPORSIRANO ?? "",
            PoliklinikTarihi = DateTime.TryParse(b.POLIKLINIKTAR, out var poliDate) ? poliDate : null,
            YatakRaporBaslangic = DateTime.TryParse(b.YATRAPBASTAR, out var yatBasDate) ? yatBasDate : null,
            YatakRaporBitis = DateTime.TryParse(b.YATRAPBITTAR, out var yatBitDate) ? yatBitDate : null,
            AyaktaBaslangic = DateTime.TryParse(b.ABASTAR, out var ayakBasDate) ? ayakBasDate : null,
            AyaktaBitis = DateTime.TryParse(b.ABITTAR, out var ayakBitDate) ? ayakBitDate : null,
            IsBasiKontrolTarihi = DateTime.TryParse(b.ISBASKONTTAR, out var kontDate) ? kontDate : null,
            Vaka = b.VAKA ?? "",
            VakaAdi = b.VAKAADI ?? "",
            RaporDurumu = b.RAPORDURUMU ?? "",
            TesisKodu = b.TESISKODU ?? "",
            TesisAdi = b.TESISADI ?? ""
        }).ToList();
    }

    private List<OnayliRaporItemDto> MapToOnayliRaporItemDtoList(OnayliRaporlarTarihleBean[]? beans)
    {
        if (beans == null) return new List<OnayliRaporItemDto>();

        return beans.Select(b => new OnayliRaporItemDto
        {
            TcKimlikNo = b.TCKIMLIKNO ?? "",
            Ad = b.AD ?? "",
            Soyad = b.SOYAD ?? "",
            MedulaRaporId = long.TryParse(b.MEDULARAPORID, out var id) ? id : 0,
            RaporTakipNo = b.RAPORTAKIPNO ?? "",
            RaporSiraNo = b.RAPORSIRANO ?? "",
            PoliklinikTarihi = DateTime.TryParse(b.POLIKLINIKTAR, out var poliDate) ? poliDate : null,
            IsBasiKontrolTarihi = DateTime.TryParse(b.ISBASKONTTAR, out var kontDate) ? kontDate : null,
            IsKazasiTarihi = DateTime.TryParse(b.ISKAZASITARIHI, out var kazaDate) ? kazaDate : null,
            Vaka = b.VAKA ?? "",
            VakaAdi = b.VAKAADI ?? ""
        }).ToList();
    }
}