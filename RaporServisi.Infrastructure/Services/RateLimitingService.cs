using Microsoft.Extensions.Logging;
using RaporServisi.Application.DTOs;

namespace RaporServisi.Infrastructure.Services;

public class SgkRateLimitingService
{
    private readonly ILogger<SgkRateLimitingService> _logger;

    // Thread-safe dictionary to track requests per işyeri
    private static readonly Dictionary<string, List<DateTime>> _requestTracker = new();
    private static readonly object _lock = new();

    public SgkRateLimitingService(ILogger<SgkRateLimitingService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Dokümana göre rate limiting kontrolü
    /// RaporAramaTarihile: 24 saatte max 2 sorgu
    /// HasIsKazSorguTarihle: 24 saatte max 2 sorgu + 15 dk aralık
    /// </summary>
    public RateLimitInfoDto CheckRateLimit(string isyeriKodu, string metodAdi)
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            var key = $"{metodAdi}_{isyeriKodu}";

            if (!_requestTracker.ContainsKey(key))
                _requestTracker[key] = new List<DateTime>();

            // 24 saat öncesindeki kayıtları temizle
            _requestTracker[key] = _requestTracker[key]
                .Where(x => (now - x).TotalHours < 24)
                .ToList();

            var recentRequests = _requestTracker[key];

            // Metodda göre farklı kurallar
            var result = metodAdi.ToLower() switch
            {
                "raporaramatarihile" => CheckRaporAramaRateLimit(recentRequests, now),
                "hasiskzasorgutarihle" => CheckHasIsKazRateLimit(recentRequests, now),
                _ => new RateLimitInfoDto
                {
                    CanMakeRequest = true,
                    RemainingRequests = 2,
                    Message = "Bilinmeyen metot için rate limit uygulanmadı"
                }
            };

            // İstek yapılabiliyorsa kaydet
            if (result.CanMakeRequest)
            {
                _requestTracker[key].Add(now);
                _logger.LogInformation("Rate limit passed for {Method} - {IsyeriKodu}", metodAdi, isyeriKodu);
            }
            else
            {
                _logger.LogWarning("Rate limit exceeded for {Method} - {IsyeriKodu}. Message: {Message}",
                    metodAdi, isyeriKodu, result.Message);
            }

            return result;
        }
    }

    private RateLimitInfoDto CheckRaporAramaRateLimit(List<DateTime> recentRequests, DateTime now)
    {
        // Aynı İşveren için son 24 saat içinde en fazla 2 sorgu yapılabilir
        if (recentRequests.Count >= 2)
        {
            var oldestRequest = recentRequests.Min();
            var nextAvailable = oldestRequest.AddHours(24);

            return new RateLimitInfoDto
            {
                CanMakeRequest = false,
                RemainingRequests = 0,
                NextAvailableTime = nextAvailable,
                Message = "24 saat içinde maksimum 2 sorgu sınırına ulaştınız. " +
                         $"Sonraki sorgu zamanı: {nextAvailable:dd.MM.yyyy HH:mm}"
            };
        }

        return new RateLimitInfoDto
        {
            CanMakeRequest = true,
            RemainingRequests = 2 - recentRequests.Count,
            Message = $"Kalan sorgu hakkı: {2 - recentRequests.Count}"
        };
    }

    private RateLimitInfoDto CheckHasIsKazRateLimit(List<DateTime> recentRequests, DateTime now)
    {
        // 24 saat içinde max 2 sorgu + 15 dakika aralık kontrolleri
        if (recentRequests.Count >= 2)
        {
            var oldestRequest = recentRequests.Min();
            var nextAvailable = oldestRequest.AddHours(24);

            return new RateLimitInfoDto
            {
                CanMakeRequest = false,
                RemainingRequests = 0,
                NextAvailableTime = nextAvailable,
                Message = "İş Kazası sorgusu için 24 saat içinde maksimum 2 sorgu sınırına ulaştınız."
            };
        }

        // 15 dakika aralık kontrolü (sadece son istek için)
        if (recentRequests.Count > 0)
        {
            var lastRequest = recentRequests.Max();
            var minutesSinceLastRequest = (now - lastRequest).TotalMinutes;

            if (minutesSinceLastRequest < 15)
            {
                var nextAvailable = lastRequest.AddMinutes(15);
                return new RateLimitInfoDto
                {
                    CanMakeRequest = false,
                    RemainingRequests = 2 - recentRequests.Count,
                    NextAvailableTime = nextAvailable,
                    Message = $"İş Kazası sorgusu için 15 dakika beklemeniz gerekiyor. " +
                             $"Sonraki sorgu zamanı: {nextAvailable:HH:mm}"
                };
            }
        }

        return new RateLimitInfoDto
        {
            CanMakeRequest = true,
            RemainingRequests = 2 - recentRequests.Count,
            Message = $"Kalan iş kazası sorgu hakkı: {2 - recentRequests.Count}"
        };
    }

    /// <summary>
    /// Test amaçlı rate limit sıfırlama (sadece development ortamında kullanılmalı)
    /// </summary>
    public void ResetRateLimit(string isyeriKodu, string metodAdi)
    {
        lock (_lock)
        {
            var key = $"{metodAdi}_{isyeriKodu}";
            if (_requestTracker.ContainsKey(key))
            {
                _requestTracker[key].Clear();
                _logger.LogWarning("Rate limit reset for {Method} - {IsyeriKodu}", metodAdi, isyeriKodu);
            }
        }
    }

    /// <summary>
    /// Tüm rate limit bilgilerini temizle (sistem yeniden başlatılırken)
    /// </summary>
    public void ClearAllRateLimits()
    {
        lock (_lock)
        {
            _requestTracker.Clear();
            _logger.LogInformation("All rate limits cleared");
        }
    }

    /// <summary>
    /// İstatistik bilgisi - hangi işyerlerinin ne kadar istek yaptığını gösterir
    /// </summary>
    public Dictionary<string, object> GetRateLimitStats()
    {
        lock (_lock)
        {
            var stats = new Dictionary<string, object>();

            foreach (var kvp in _requestTracker)
            {
                var key = kvp.Key;
                var requests = kvp.Value.Where(x => (DateTime.UtcNow - x).TotalHours < 24).ToList();

                stats[key] = new
                {
                    RequestCount = requests.Count,
                    LastRequest = requests.Count > 0 ? requests.Max().ToString("dd.MM.yyyy HH:mm:ss") : "Hiç",
                    CanMakeRequest = requests.Count < 2
                };
            }

            return stats;
        }
    }
}