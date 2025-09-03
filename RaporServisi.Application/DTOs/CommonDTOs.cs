namespace RaporServisi.Application.DTOs;

// Genel amaçlı DTOs
public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public T? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public List<string> Errors { get; set; } = new();
}

public class SgkOperationResultDto
{
    public bool Success { get; set; }
    public int SonucKod { get; set; }
    public string SonucAciklama { get; set; } = "";
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}

public class RateLimitInfoDto
{
    public bool CanMakeRequest { get; set; }
    public int RemainingRequests { get; set; }
    public DateTime? NextAvailableTime { get; set; }
    public string Message { get; set; } = "";
}

// Dokümandaki parametrik alanlar için enum'lar
public enum VakaKodu
{
    IsKazasi = 1,
    MeslekHastaligi = 2,
    Hastalik = 3,
    Analik = 4
}

public enum RaporDurumu
{
    Calisir = 1,
    Kontrol = 2,
    DevamVerildi = 3,
    Sevkli = 4,
    HastaneKapatti = 5,
    CalisirCakismaVar = 6,
    KontrolCakismaVar = 7,
    MaluliyetAzaltilabilirCalisir = 8,
    MaluliyetSevkCalisir = 9,
    AnalikDoğumOncesiCalisir = 10,
    AnalikDoğumOncesiCalismaz = 11,
    AnalikDoğumSonrasi = 12,
    MaluliyetAzaltilirKontrol = 13,
    MaluliyetSevkKontrol = 14,
    MaluliyetAzaltilirKontrolDevamVerildi = 15,
    MaluliyetSevkKontrolDevamVerildi = 16
}

public static class VakaHelper
{
    public static string GetVakaAdi(int? vakaKodu)
    {
        return vakaKodu switch
        {
            1 => "İş Kazası",
            2 => "Meslek Hastalığı",
            3 => "Hastalık",
            4 => "Analık",
            _ => "Bilinmiyor"
        };
    }

    public static string GetRaporDurumuAdi(int? durum)
    {
        return durum switch
        {
            1 => "Çalışır",
            2 => "Kontrol",
            3 => "Devamı Verildi",
            4 => "Sevkli",
            5 => "Hastane Kapattı",
            6 => "Çalışır Olup Çakışma Var",
            7 => "Kontrol olup çakışma var",
            8 => "Maluliyet Azaltılabilir çalışır",
            9 => "Maluliyet Sevk Çalışır",
            10 => "Analık Doğum Öncesi Çalışır",
            11 => "Analık Doğum Öncesi Çalışamaz",
            12 => "Analık Doğum Sonrası",
            13 => "Maluliyet Azaltılır Kontrol",
            14 => "Maluliyet Sevk Kontrol",
            15 => "Maluliyet Azaltılır Kontrol Devam Verildi",
            16 => "Maluliyet Sevk Kontrol Devam Verildi",
            _ => "Bilinmiyor"
        };
    }
}