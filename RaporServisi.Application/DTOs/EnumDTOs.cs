namespace RaporServisi.Application.DTOs;

// SGK Sonuç Kodları - Belgeden alınan tüm hata kodları
public enum SgkResultCode
{
    Success = 0,

    // Login hataları (101-106)
    UserNameEmpty = 101,
    UserCodeEmpty = 102,
    PasswordEmpty = 103,
    TokenEmpty = 104,
    InvalidCredentials = 105,
    TokenExpired = 106,

    // Parametre hataları (201-212)
    PhoneNumberEmpty = 201,
    EmailEmpty = 202,
    IdEmpty = 203,
    StartDateEmpty = 204,
    ReportIdEmpty = 205,
    StatusEmpty = 206,
    ReportStartDateEmpty = 207,
    DateEmpty = 208,
    Date2Empty = 209,
    TcIdentityNumberEmpty = 210,
    CaseTypeEmpty = 211,
    NotificationIdEmpty = 212,

    // Format hataları (301-308)
    StartDateFormatInvalid = 301,
    ReportStartDateFormatInvalid = 302,
    DateFormatInvalid = 303,
    Date2FormatInvalid = 304,
    DateRangeExceeded = 305,
    PaymentStartDateFormatInvalid = 306,
    PaymentEndDateFormatInvalid = 307,
    PaymentAmountEmpty = 308,

    // Doğrulama hataları (401-403)
    StatusMustBe0Or1 = 401,
    TcIdentityNumberLengthError = 402,
    CaseTypeMustBe1234 = 403,

    // Kayıt bulunamadı hataları (501-505)
    RecordNotFound = 501,
    ReportNotFound = 502,
    NoRecordsInDateRange = 503,
    NoRecordsForDateRange = 504,
    NoRecordsForTcIdentityNumber = 505,

    // Transfer hataları (600-602)
    TransferredSuccessfully = 600,
    TransferSuccessful = 601,
    TransferFailedEmployeeExists = 602,

    // Business logic hataları (801-825)
    DateCannotBeGreaterThanToday = 801,
    DateCannotBeGreaterThanReportEndDate = 802,
    DateCannotBeLessThanClinicDate = 803,
    ShortTermReportsCanBeApproved = 804,
    DateRangeMustBeAtLeast10Days = 805,
    RemainingReportDurationMustBeAtLeast10Days = 806,
    ReportEndDateCannotBeEmpty = 809,
    NoWorkDaysAvailable = 810,
    PregnancyReportError = 825,

    // İşlem durumu hataları (901-911)
    EmployeeNotFoundInCompany = 901,
    ReportNotBelongToCompany = 902,
    ReportAlreadyApproved = 904,
    ReportPaymentMade = 905,
    EmployeeReportExistsInDateRange = 906,
    OperationFailed = 907,
    OperationNotCompleted = 908,
    CannotDeleteRecord = 909,
    EmployeeNotWorkingInCompany = 910,
    ReportCannotBeClosed = 911,

    // İş kazası hataları (921-922)
    WorkAccidentProvisionCannotBeClosed = 921,
    WorkAccidentProvisionNotBelongToCompany = 922,

    // Rate limiting hataları (1010-1011)
    MaximumQueryLimitReached = 1010,
    WaitBetweenQueries = 1011,

    // Mahsuplaşma hataları (1000-1008)
    NoOffsetAgreement = 1000,
    TcIdentityNumberMismatch = 1001,
    CaseTypeMismatch = 1002,
    StartDateMismatch = 1003,
    EndDateMismatch = 1004,
    AmountMismatch = 1005,
    RecordNotFoundForOffset = 1006,
    NoPaymentRecordInMosip = 1007,
    AgreementNotFoundInMosip = 1008
}

public enum CaseType
{
    WorkAccident = 1,
    OccupationalDisease = 2,
    Illness = 3,
    Maternity = 4
}

// Rapor Durumları
public enum ReportStatus
{
    CanWork = 1,
    Control = 2,
    ContinuationGiven = 3,
    Referred = 4,
    HospitalClosed = 5,
    CanWorkWithConflict = 6,
    ControlWithConflict = 7,
    DisabilityReducibleCanWork = 8,
    DisabilityReferredCanWork = 9,
    MaternityPreBirthCanWork = 10,
    MaternityPreBirthCannotWork = 11,
    MaternityPostBirth = 12,
    DisabilityReducedControl = 13,
    DisabilityReferredControl = 14,
    DisabilityReducedControlContinuation = 15,
    DisabilityReferredControlContinuation = 16
}

// Nitelik Durumu (Çalışma Durumu)
public enum WorkStatus
{
    DidNotWork = 0,
    Worked = 1
}

// Helper sınıfları - Türkçe mesajlar
public static class SgkResultCodeHelper
{
    private static readonly Dictionary<SgkResultCode, string> _messages = new()
    {
        // Başarılı işlemler
        { SgkResultCode.Success, "İşlem başarılı" },
        { SgkResultCode.TransferredSuccessfully, "Başarıyla aktarılmıştır" },
        { SgkResultCode.TransferSuccessful, "Başarılı ile aktarılmıştır" },
        
        // Login hataları
        { SgkResultCode.UserNameEmpty, "Kullanıcı Adı Boş Olamaz" },
        { SgkResultCode.UserCodeEmpty, "Kullanıcı Kodu Boş Olamaz" },
        { SgkResultCode.PasswordEmpty, "Şifre Boş Olamaz" },
        { SgkResultCode.TokenEmpty, "Token Boş Olamaz" },
        { SgkResultCode.InvalidCredentials, "Kullanıcı Adı, Kullanıcı Kodu veya Şifre hatalı. Tekrar deneyin" },
        { SgkResultCode.TokenExpired, "Kullanıcı Adı, Kullanıcı Kodu, Token hatalı veya Token süresi dolmuştur. Tekrar token alınız" },
        
        // Parametre hataları
        { SgkResultCode.PhoneNumberEmpty, "Cep Telefonu Boş Olamaz" },
        { SgkResultCode.EmailEmpty, "Eposta Boş Olamaz" },
        { SgkResultCode.IdEmpty, "ID Boş Olamaz" },
        { SgkResultCode.StartDateEmpty, "İşe Başlama Tarihi Boş Olamaz" },
        { SgkResultCode.ReportIdEmpty, "Medula Rapor Id Boş Olamaz" },
        { SgkResultCode.StatusEmpty, "Nitelik Durumu Boş Olamaz" },
        { SgkResultCode.ReportStartDateEmpty, "Rapor Başlangıç Tarihi Boş Olamaz" },
        { SgkResultCode.DateEmpty, "Tarih Boş Olamaz" },
        { SgkResultCode.Date2Empty, "Tarih2 Boş Olamaz" },
        { SgkResultCode.TcIdentityNumberEmpty, "TC Kimlik Numarası Boş Olamaz" },
        { SgkResultCode.CaseTypeEmpty, "Vaka Boş Olamaz" },
        { SgkResultCode.NotificationIdEmpty, "Bildirim Id Boş Olamaz" },
        
        // Format hataları
        { SgkResultCode.StartDateFormatInvalid, "İşe Başlama Tarihi formatınız doğru değil. Format (dd.mm.yyyy) olmalı" },
        { SgkResultCode.ReportStartDateFormatInvalid, "Rapor Başlangıç Tarihi formatınız doğru değil. Format (dd.mm.yyyy) olmalı" },
        { SgkResultCode.DateFormatInvalid, "Tarih formatınız doğru değil. Format (dd.mm.yyyy) olmalı" },
        { SgkResultCode.Date2FormatInvalid, "Tarih2 formatınız doğru değil. Format (dd.mm.yyyy) olmalı" },
        { SgkResultCode.DateRangeExceeded, "Gün Farkı 1 aydan büyük olamaz!" },
        { SgkResultCode.PaymentStartDateFormatInvalid, "Ödeme Başlangıç Tarihi formatınız doğru değil. Format (dd.mm.yyyy) olmalı" },
        { SgkResultCode.PaymentEndDateFormatInvalid, "Ödeme Bitiş Tarihi formatınız doğru değil. Format (dd.mm.yyyy) olmalı" },
        { SgkResultCode.PaymentAmountEmpty, "Ödenen Tutar String Boş Olamaz" },
        
        // Doğrulama hataları
        { SgkResultCode.StatusMustBe0Or1, "Nitelik Durumu 0 veya 1 olmalıdır" },
        { SgkResultCode.TcIdentityNumberLengthError, "TC Kimlik Numarası Uzunluk Hatası" },
        { SgkResultCode.CaseTypeMustBe1234, "Vaka türü 1,2,3,4 olabilir" },
        
        // Kayıt bulunamadı hataları
        { SgkResultCode.RecordNotFound, "Kayıt Bulunamadı" },
        { SgkResultCode.ReportNotFound, "Rapor bulunamadı" },
        { SgkResultCode.NoRecordsInDateRange, "Sorguladığınız Tarih Aralığında Kayıt Bulunamadı" },
        { SgkResultCode.NoRecordsForTcIdentityNumber, "Sorguladığınız TC Kimlik No için Kayıt Bulunamadı" },
        
        // Business logic hataları
        { SgkResultCode.DateCannotBeGreaterThanToday, "Girdiğiniz Tarih Günün Tarihinden Büyük Olamaz" },
        { SgkResultCode.DateCannotBeGreaterThanReportEndDate, "Girdiğiniz Tarih, Rapor Bitiş Tarihinden Büyük Olamaz" },
        { SgkResultCode.DateCannotBeLessThanClinicDate, "Girdiğiniz Tarih, Poliklinik Tarihinden Küçük olamaz" },
        { SgkResultCode.ShortTermReportsCanBeApproved, "10 gün ve daha kısa süreli raporları tek seferde onaylanabilir" },
        { SgkResultCode.DateRangeMustBeAtLeast10Days, "Tarih Aralığı En az 10 Gün Olmalıdır" },
        { SgkResultCode.RemainingReportDurationMustBeAtLeast10Days, "Raporun Kalan Süresi En Az 10 Gün Olmalıdır" },
        { SgkResultCode.ReportEndDateCannotBeEmpty, "Rapor bitiş tarihi boş olamaz!" },
        { SgkResultCode.NoWorkDaysAvailable, "Bildirim yapılacak gün bulunmamaktadır" },
        { SgkResultCode.PregnancyReportError, "Aktarma süresinde doğum gerçekleştiğinden doğum öncesinde istirahat bulunmamaktadır. Rapor Arşive kaldırılmıştır!" },
        
        // İşlem durumu hataları
        { SgkResultCode.EmployeeNotFoundInCompany, "Girilen sigortalı bilgisi ilgili işyerinde çalışır görünmüyor" },
        { SgkResultCode.ReportNotBelongToCompany, "Rapor işyerine ait gözükmüyor" },
        { SgkResultCode.ReportAlreadyApproved, "Rapor zaten onaylı değil" },
        { SgkResultCode.ReportPaymentMade, "Raporun Ödemesi Yapılmış, Onay İptal Edilemez" },
        { SgkResultCode.EmployeeReportExistsInDateRange, "Sigortalıya ait girilen tarih aralığında bildirim bulunmaktadır" },
        { SgkResultCode.OperationFailed, "Başarısız" },
        { SgkResultCode.OperationNotCompleted, "İşlem Tamamlanamadı" },
        { SgkResultCode.CannotDeleteRecord, "Silinemememiştir" },
        { SgkResultCode.EmployeeNotWorkingInCompany, "TC Kimlik Numaralı sigortalı bu işyerinde çalışmıyor" },
        { SgkResultCode.ReportCannotBeClosed, "Rapor Kapatılamamıştır" },
        
        // İş kazası hataları
        { SgkResultCode.WorkAccidentProvisionCannotBeClosed, "İş Kazası Hastane Provizyonu Kapatılamamıştır" },
        { SgkResultCode.WorkAccidentProvisionNotBelongToCompany, "İş Kazası Hastane Provizyonu İşyerine Ait Gözükmüyor" },
        
        // Rate limiting hataları
        { SgkResultCode.MaximumQueryLimitReached, "Maksimum sorgu sayısına ulaştınız. (Aynı İşveren için son 24 saat içinde en fazla 2 sorgu yapılabilir.)" },
        { SgkResultCode.WaitBetweenQueries, "dakika aralıklar ile sorgulama yapabilirsiniz. !!!!" },
        
        // Mahsuplaşma hataları
        { SgkResultCode.NoOffsetAgreement, "SGK ile mahsuplaşma anlaşmanız bulunmamaktadır. Anlaşma yapıldığı takdirde bu metodları kullanabilirsiniz!" },
        { SgkResultCode.TcIdentityNumberMismatch, "Girilen TC Kimlik Numarası Uyuşmuyor" },
        { SgkResultCode.CaseTypeMismatch, "Girilen Vaka Uyuşmuyor" },
        { SgkResultCode.StartDateMismatch, "Girilen tarih ile ödeme Başlangıç Tarihi Uyuşmuyor" },
        { SgkResultCode.EndDateMismatch, "Girilen tarih ile ödeme Bitiş Tarihi Uyuşmuyor" },
        { SgkResultCode.AmountMismatch, "Girilen tutar ile ödenek tutarı Uyuşmuyor" },
        { SgkResultCode.RecordNotFoundForOffset, "Böyle bir kayıt bulunamadı" },
        { SgkResultCode.NoPaymentRecordInMosip, "Seçilen Rapor Bilgisi İçin MOSİP Sisteminde Ödeme Kaydı Bulunamadı!! Mahsuplaşma Yapamazsız!" },
        { SgkResultCode.AgreementNotFoundInMosip, "İşyeri Bilgilerine Karşılık Gelen Anlaşma Mosip Tarafında Bulunamadı!!" }
    };

    public static string GetMessage(SgkResultCode code)
    {
        return _messages.TryGetValue(code, out var message) ? message : "Bilinmeyen hata";
    }

    public static string GetMessage(int code)
    {
        if (Enum.IsDefined(typeof(SgkResultCode), code))
        {
            return GetMessage((SgkResultCode)code);
        }
        return $"Bilinmeyen hata kodu: {code}";
    }

    public static bool IsSuccess(SgkResultCode code)
    {
        return code == SgkResultCode.Success ||
               code == SgkResultCode.TransferredSuccessfully ||
               code == SgkResultCode.TransferSuccessful;
    }

    public static bool IsSuccess(int code)
    {
        return code == 0 || code == 600 || code == 601;
    }
}

public static class CaseTypeHelper
{
    public static string GetDescription(CaseType caseType)
    {
        return caseType switch
        {
            CaseType.WorkAccident => "İş Kazası",
            CaseType.OccupationalDisease => "Meslek Hastalığı",
            CaseType.Illness => "Hastalık",
            CaseType.Maternity => "Analık",
            _ => "Bilinmiyor"
        };
    }

    public static string GetDescription(string caseCode)
    {
        return caseCode switch
        {
            "1" => "İş Kazası",
            "2" => "Meslek Hastalığı",
            "3" => "Hastalık",
            "4" => "Analık",
            _ => "Bilinmiyor"
        };
    }
}

public static class ReportStatusHelper
{
    public static string GetDescription(ReportStatus status)
    {
        return status switch
        {
            ReportStatus.CanWork => "Çalışır",
            ReportStatus.Control => "Kontrol",
            ReportStatus.ContinuationGiven => "Devamı Verildi",
            ReportStatus.Referred => "Sevkli",
            ReportStatus.HospitalClosed => "Hastane Kapattı",
            ReportStatus.CanWorkWithConflict => "Çalışır Olup Çakışma Var",
            ReportStatus.ControlWithConflict => "Kontrol olup çakışma var",
            ReportStatus.DisabilityReducibleCanWork => "Maluliyet Azaltılabilir çalışır",
            ReportStatus.DisabilityReferredCanWork => "Maluliyet Sevk Çalışır",
            ReportStatus.MaternityPreBirthCanWork => "Analık Doğum Öncesi Çalışır",
            ReportStatus.MaternityPreBirthCannotWork => "Analık Doğum Öncesi Çalışamaz",
            ReportStatus.MaternityPostBirth => "Analık Doğum Sonrası",
            ReportStatus.DisabilityReducedControl => "Maluliyet Azaltılır Kontrol",
            ReportStatus.DisabilityReferredControl => "Maluliyet Sevk Kontrol",
            ReportStatus.DisabilityReducedControlContinuation => "Maluliyet Azaltılır Kontrol Devam Verildi",
            ReportStatus.DisabilityReferredControlContinuation => "Maluliyet Sevk Kontrol Devam Verildi",
            _ => "Bilinmiyor"
        };
    }

    public static string GetDescription(string statusCode)
    {
        if (int.TryParse(statusCode, out var code) && Enum.IsDefined(typeof(ReportStatus), code))
        {
            return GetDescription((ReportStatus)code);
        }
        return "Bilinmiyor";
    }
}

public static class WorkStatusHelper
{
    public static string GetDescription(WorkStatus status)
    {
        return status switch
        {
            WorkStatus.DidNotWork => "Çalışmamıştır",
            WorkStatus.Worked => "Çalışmıştır",
            _ => "Bilinmiyor"
        };
    }

    public static string GetDescription(string statusCode)
    {
        return statusCode switch
        {
            "0" => "Çalışmamıştır",
            "1" => "Çalışmıştır",
            _ => "Bilinmiyor"
        };
    }
}