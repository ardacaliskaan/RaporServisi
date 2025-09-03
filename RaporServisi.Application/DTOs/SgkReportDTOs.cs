using System.ComponentModel.DataAnnotations;

namespace RaporServisi.Application.DTOs;

// Request DTOs
public class SgkLoginRequestDto
{
    [Required(ErrorMessage = "Kullanıcı adı zorunludur")]
    public string KullaniciAdi { get; set; } = "";

    [Required(ErrorMessage = "İşyeri kodu zorunludur")]
    public string IsyeriKodu { get; set; } = "";

    [Required(ErrorMessage = "WS şifre zorunludur")]
    public string WsSifre { get; set; } = "";
}

public class RaporAramaRequestDto : SgkLoginRequestDto
{
    [Required(ErrorMessage = "Tarih zorunludur")]
    [RegularExpression(@"^\d{2}\.\d{2}\.\d{4}$", ErrorMessage = "Tarih formatı dd.MM.yyyy olmalıdır")]
    public string Tarih { get; set; } = "";
}

public class OnayliRaporlarRequestDto : SgkLoginRequestDto
{
    [Required(ErrorMessage = "Başlangıç tarihi zorunludur")]
    [RegularExpression(@"^\d{2}\.\d{2}\.\d{4}$", ErrorMessage = "Tarih formatı dd.MM.yyyy olmalıdır")]
    public string BaslangicTarihi { get; set; } = "";

    [Required(ErrorMessage = "Bitiş tarihi zorunludur")]
    [RegularExpression(@"^\d{2}\.\d{2}\.\d{4}$", ErrorMessage = "Tarih formatı dd.MM.yyyy olmalıdır")]
    public string BitisTarihi { get; set; } = "";
}

public class RaporOkunduKapatRequestDto : SgkLoginRequestDto
{
    [Required(ErrorMessage = "Medula rapor ID zorunludur")]
    [Range(1, long.MaxValue, ErrorMessage = "Geçerli bir rapor ID giriniz")]
    public long MedulaRaporId { get; set; }
}

public class RaporOnayRequestDto : SgkLoginRequestDto
{
    [Required(ErrorMessage = "TC kimlik numarası zorunludur")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "TC kimlik numarası 11 karakter olmalıdır")]
    public string TcKimlikNo { get; set; } = "";

    [Required(ErrorMessage = "Vaka kodu zorunludur")]
    [RegularExpression("^[1-4]$", ErrorMessage = "Vaka: 1-İş Kazası, 2-Meslek Hastalığı, 3-Hastalık, 4-Analık")]
    public string Vaka { get; set; } = "";

    [Required(ErrorMessage = "Medula rapor ID zorunludur")]
    [Range(1, long.MaxValue, ErrorMessage = "Geçerli bir rapor ID giriniz")]
    public long MedulaRaporId { get; set; }

    [Required(ErrorMessage = "Nitelik durumu zorunludur")]
    [RegularExpression("^[01]$", ErrorMessage = "Nitelik durumu: 0-Çalışmamış, 1-Çalışmış")]
    public string NitelikDurumu { get; set; } = "";

    [Required(ErrorMessage = "Tarih zorunludur")]
    [RegularExpression(@"^\d{2}\.\d{2}\.\d{4}$", ErrorMessage = "Tarih formatı dd.MM.yyyy olmalıdır")]
    public string Tarih { get; set; } = "";
}

// Response DTOs
public class SgkBaseResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}

public class RaporItemDto
{
    public string TcKimlikNo { get; set; } = "";
    public string Ad { get; set; } = "";
    public string Soyad { get; set; } = "";
    public string AdSoyad => $"{Ad?.Trim()} {Soyad?.Trim()}".Trim();
    public long MedulaRaporId { get; set; }
    public string RaporTakipNo { get; set; } = "";
    public string RaporSiraNo { get; set; } = "";
    public DateTime? PoliklinikTarihi { get; set; }
    public DateTime? YatakRaporBaslangic { get; set; }
    public DateTime? YatakRaporBitis { get; set; }
    public DateTime? AyaktaBaslangic { get; set; }
    public DateTime? AyaktaBitis { get; set; }
    public DateTime? IsBasiKontrolTarihi { get; set; }
    public string Vaka { get; set; } = "";
    public string VakaAdi { get; set; } = "";
    public string RaporDurumu { get; set; } = "";
    public string TesisKodu { get; set; } = "";
    public string TesisAdi { get; set; } = "";
}

public class RaporAramaResponseDto : SgkBaseResponseDto
{
    public List<RaporItemDto> Raporlar { get; set; } = new();
    public int ToplamRapor { get; set; }
    public string SorguTarihi { get; set; } = "";
}

public class OnayliRaporlarResponseDto : SgkBaseResponseDto
{
    public List<OnayliRaporItemDto> Raporlar { get; set; } = new();
    public int ToplamRapor { get; set; }
    public string BaslangicTarihi { get; set; } = "";
    public string BitisTarihi { get; set; } = "";
}

public class OnayliRaporItemDto
{
    public string TcKimlikNo { get; set; } = "";
    public string Ad { get; set; } = "";
    public string Soyad { get; set; } = "";
    public string AdSoyad => $"{Ad?.Trim()} {Soyad?.Trim()}".Trim();
    public long MedulaRaporId { get; set; }
    public string RaporTakipNo { get; set; } = "";
    public string RaporSiraNo { get; set; } = "";
    public DateTime? PoliklinikTarihi { get; set; }
    public DateTime? IsBasiKontrolTarihi { get; set; }
    public DateTime? IsKazasiTarihi { get; set; }
    public string Vaka { get; set; } = "";
    public string VakaAdi { get; set; } = "";
}