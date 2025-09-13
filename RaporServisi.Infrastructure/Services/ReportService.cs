using Microsoft.Extensions.Logging;
using RaporServisi.Application.DTOs;
using RaporServisi.Application.Services;
using RaporServisi.Application.Utilities;
using SgkVizite;
using System.Diagnostics;

namespace RaporServisi.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly ViziteGonderClient _soapClient;
    private readonly ILogger<ReportService> _logger;

    public ReportService(ViziteGonderClient soapClient, ILogger<ReportService> logger)
    {
        _soapClient = soapClient;
        _logger = logger;
    }

    public async Task<ApiResponseDto<LoginResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("SGK Login işlemi başlatılıyor - İşyeri: {CompanyCode}", request.CompanyCode);

            var result = await _soapClient.wsLoginAsync(request.Username, request.CompanyCode, request.WsPassword);
            var response = result?.wsLoginReturn;

            var loginResponse = new LoginResponseDto
            {
                Success = response?.sonucKod == 0,
                ResultCode = response?.sonucKod ?? -1,
                Message = SgkResultCodeHelper.GetMessage(response?.sonucKod ?? -1),
                Token = response?.wsToken
            };

            if (loginResponse.Success)
            {
                _logger.LogInformation("SGK Login başarılı - İşyeri: {CompanyCode}", request.CompanyCode);
            }
            else
            {
                _logger.LogWarning("SGK Login başarısız - İşyeri: {CompanyCode}, Kod: {ResultCode}",
                    request.CompanyCode, loginResponse.ResultCode);
            }

            return new ApiResponseDto<LoginResponseDto>
            {
                Success = loginResponse.Success,
                Message = loginResponse.Message,
                Data = loginResponse
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SGK Login işleminde hata - İşyeri: {CompanyCode}", request.CompanyCode);
            return new ApiResponseDto<LoginResponseDto>
            {
                Success = false,
                Message = "Login işleminde sistem hatası",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponseDto<ReportSearchResponseDto>> SearchReportsByDateAsync(
        ReportSearchRequestDto request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Rapor arama işlemi başlatılıyor - İşyeri: {CompanyCode}, Tarih: {Date}",
                request.CompanyCode, request.Date);

            // Login
            var loginResult = await LoginAsync(new LoginRequestDto
            {
                Username = request.Username,
                CompanyCode = request.CompanyCode,
                WsPassword = request.WsPassword
            }, ct);

            if (!loginResult.Success || string.IsNullOrWhiteSpace(loginResult.Data?.Token))
            {
                return new ApiResponseDto<ReportSearchResponseDto>
                {
                    Success = false,
                    Message = "Login başarısız: " + loginResult.Message,
                    Errors = loginResult.Errors
                };
            }

            var token = loginResult.Data.Token;

            // RaporAramaTarihile çağrısı
            var result = await _soapClient.raporAramaTarihileAsync(
                request.Username, request.CompanyCode, token, request.Date);

            var sgkResponse = result?.raporAramaTarihileReturn;
            var response = new ReportSearchResponseDto
            {
                Success = sgkResponse?.sonucKod == 0,
                Message = SgkResultCodeHelper.GetMessage(sgkResponse?.sonucKod ?? -1),
                SearchDate = request.Date,
                SearchCriteria = $"Tarih: {request.Date}"
            };

            if (response.Success && sgkResponse?.raporAramaTarihleBeanArray != null)
            {
                // Success/Error list pattern - Notlardaki gereksinim
                foreach (var item in sgkResponse.raporAramaTarihleBeanArray)
                {
                    try
                    {
                        var reportItem = MapToReportItem(item);
                        response.SuccessList.Add(reportItem);
                    }
                    catch (Exception ex)
                    {
                        response.ErrorList.Add(new ReportErrorDto
                        {
                            TcIdentityNumber = item?.TCKIMLIKNO,
                            EmployeeName = $"{item?.AD} {item?.SOYAD}".Trim(),
                            ReportId = long.TryParse(item?.MEDULARAPORID, out var id) ? id : null,
                            ErrorCode = -1,
                            ErrorMessage = $"Veri dönüştürme hatası: {ex.Message}"
                        });
                    }
                }

                response.Message = $"{response.SuccessCount} rapor başarıyla alındı, {response.ErrorCount} hatası var";
            }
            else if (!response.Success)
            {
                response.ErrorList.Add(new ReportErrorDto
                {
                    ErrorCode = sgkResponse?.sonucKod ?? -1,
                    ErrorMessage = response.Message
                });
            }

            _logger.LogInformation("Rapor arama tamamlandı - Başarılı: {SuccessCount}, Hata: {ErrorCount}",
                response.SuccessCount, response.ErrorCount);

            return new ApiResponseDto<ReportSearchResponseDto>
            {
                Success = response.Success || response.SuccessCount > 0,
                Message = response.Message,
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rapor arama işleminde hata - İşyeri: {CompanyCode}", request.CompanyCode);
            return new ApiResponseDto<ReportSearchResponseDto>
            {
                Success = false,
                Message = "Rapor arama işleminde sistem hatası",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponseDto<ApprovedReportsResponseDto>> GetApprovedReportsAsync(
        ApprovedReportsRequestDto request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Onaylı rapor sorgusu başlatılıyor - İşyeri: {CompanyCode}", request.CompanyCode);

            // Notlardaki kural: "girilen tarih - 5 yıl yapılacak"
            string startDate, endDate;

            if (!string.IsNullOrEmpty(request.StartDate) && !string.IsNullOrEmpty(request.EndDate))
            {
                // Manuel tarih aralığı belirtilmiş
                startDate = request.StartDate;
                endDate = request.EndDate;
                DateHelper.ValidateDateRange(startDate, endDate);
            }
            else
            {
                // Otomatik 5 yıl hesaplama
                (startDate, endDate) = DateHelper.CalculateFiveYearRange(request.Date);
            }

            // Login
            var loginResult = await LoginAsync(new LoginRequestDto
            {
                Username = request.Username,
                CompanyCode = request.CompanyCode,
                WsPassword = request.WsPassword
            }, ct);

            if (!loginResult.Success || string.IsNullOrWhiteSpace(loginResult.Data?.Token))
            {
                return new ApiResponseDto<ApprovedReportsResponseDto>
                {
                    Success = false,
                    Message = "Login başarısız: " + loginResult.Message
                };
            }

            var token = loginResult.Data.Token;

            // OnaylıRaporlarTarihile çağrısı
            var result = await _soapClient.onayliRaporlarTarihileAsync(
                request.Username, request.CompanyCode, token, startDate, endDate);

            var sgkResponse = result?.onayliRaporlarTarihileReturn;
            var response = new ApprovedReportsResponseDto
            {
                Success = sgkResponse?.sonucKod == 0,
                Message = SgkResultCodeHelper.GetMessage(sgkResponse?.sonucKod ?? -1),
                StartDate = startDate,
                EndDate = endDate,
                OriginalDate = request.Date,
                UsedAutoDateCalculation = string.IsNullOrEmpty(request.StartDate) || string.IsNullOrEmpty(request.EndDate)
            };

            if (response.Success && sgkResponse?.onayliRaporlarTarihleBeanArray != null)
            {
                foreach (var item in sgkResponse.onayliRaporlarTarihleBeanArray)
                {
                    try
                    {
                        var reportItem = MapToApprovedReportItem(item);
                        response.SuccessList.Add(reportItem);
                    }
                    catch (Exception ex)
                    {
                        response.ErrorList.Add(new ReportErrorDto
                        {
                            TcIdentityNumber = item?.TCKIMLIKNO,
                            EmployeeName = $"{item?.AD} {item?.SOYAD}".Trim(),
                            ReportId = long.TryParse(item?.MEDULARAPORID, out var id) ? id : null,
                            ErrorCode = -1,
                            ErrorMessage = $"Veri dönüştürme hatası: {ex.Message}"
                        });
                    }
                }

                response.Message = $"{response.SuccessCount} onaylı rapor başarıyla alındı, {response.ErrorCount} hata";
            }

            return new ApiResponseDto<ApprovedReportsResponseDto>
            {
                Success = response.Success || response.SuccessCount > 0,
                Message = response.Message,
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Onaylı rapor sorgusu hatası - İşyeri: {CompanyCode}", request.CompanyCode);
            return new ApiResponseDto<ApprovedReportsResponseDto>
            {
                Success = false,
                Message = "Onaylı rapor sorgusu başarısız",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponseDto<ReportOperationResponseDto>> CloseReportAsync(
        CloseReportRequestDto request, CancellationToken ct = default)
    {
        try
        {
            var loginResult = await LoginAsync(new LoginRequestDto
            {
                Username = request.Username,
                CompanyCode = request.CompanyCode,
                WsPassword = request.WsPassword
            }, ct);

            if (!loginResult.Success || string.IsNullOrWhiteSpace(loginResult.Data?.Token))
            {
                return new ApiResponseDto<ReportOperationResponseDto>
                {
                    Success = false,
                    Message = "Login başarısız: " + loginResult.Message
                };
            }

            var token = loginResult.Data.Token;
            var result = await _soapClient.raporOkunduKapatAsync(
                request.Username, request.CompanyCode, token, request.ReportId.ToString());

            var sgkResponse = result?.raporOkunduKapatReturn;
            var response = new ReportOperationResponseDto
            {
                Success = sgkResponse?.sonucKod == 0,
                ResultCode = sgkResponse?.sonucKod ?? -1,
                Message = SgkResultCodeHelper.GetMessage(sgkResponse?.sonucKod ?? -1),
                ReportId = request.ReportId
            };

            return new ApiResponseDto<ReportOperationResponseDto>
            {
                Success = response.Success,
                Message = response.Message,
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rapor kapatma hatası - RaporId: {ReportId}", request.ReportId);
            return new ApiResponseDto<ReportOperationResponseDto>
            {
                Success = false,
                Message = "Rapor kapatma işleminde hata",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponseDto<ReportOperationResponseDto>> ApproveReportAsync(
        ReportApprovalRequestDto request, CancellationToken ct = default)
    {
        try
        {
            var loginResult = await LoginAsync(new LoginRequestDto
            {
                Username = request.Username,
                CompanyCode = request.CompanyCode,
                WsPassword = request.WsPassword
            }, ct);

            if (!loginResult.Success || string.IsNullOrWhiteSpace(loginResult.Data?.Token))
            {
                return new ApiResponseDto<ReportOperationResponseDto>
                {
                    Success = false,
                    Message = "Login başarısız: " + loginResult.Message
                };
            }

            var token = loginResult.Data.Token;
            var result = await _soapClient.raporOnayAsync(
                request.Username, request.CompanyCode, token, request.TcIdentityNumber,
                request.CaseType, request.ReportId.ToString(), request.Status, request.Date);

            var sgkResponse = result?.raporOnayReturn;
            var response = new ReportOperationResponseDto
            {
                Success = sgkResponse?.sonucKod == 0,
                ResultCode = sgkResponse?.sonucKod ?? -1,
                Message = SgkResultCodeHelper.GetMessage(sgkResponse?.sonucKod ?? -1),
                ReportId = request.ReportId
            };

            return new ApiResponseDto<ReportOperationResponseDto>
            {
                Success = response.Success,
                Message = response.Message,
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rapor onay hatası - RaporId: {ReportId}", request.ReportId);
            return new ApiResponseDto<ReportOperationResponseDto>
            {
                Success = false,
                Message = "Rapor onay işleminde hata",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponseDto<ReportOperationResponseDto>> CancelReportApprovalAsync(
        ReportCancellationRequestDto request, CancellationToken ct = default)
    {
        try
        {
            var loginResult = await LoginAsync(new LoginRequestDto
            {
                Username = request.Username,
                CompanyCode = request.CompanyCode,
                WsPassword = request.WsPassword
            }, ct);

            if (!loginResult.Success || string.IsNullOrWhiteSpace(loginResult.Data?.Token))
            {
                return new ApiResponseDto<ReportOperationResponseDto>
                {
                    Success = false,
                    Message = "Login başarısız: " + loginResult.Message
                };
            }

            var token = loginResult.Data.Token;
            var result = await _soapClient.onaylIptalAsync(
                request.Username, request.CompanyCode, token, request.ReportId.ToString(), "");

            var sgkResponse = result?.onaylIptalReturn;
            var response = new ReportOperationResponseDto
            {
                Success = sgkResponse?.sonucKod == 0,
                ResultCode = sgkResponse?.sonucKod ?? -1,
                Message = SgkResultCodeHelper.GetMessage(sgkResponse?.sonucKod ?? -1),
                ReportId = request.ReportId
            };

            return new ApiResponseDto<ReportOperationResponseDto>
            {
                Success = response.Success,
                Message = response.Message,
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rapor iptal hatası - RaporId: {ReportId}", request.ReportId);
            return new ApiResponseDto<ReportOperationResponseDto>
            {
                Success = false,
                Message = "Rapor iptal işleminde hata",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    // BULK OPERATIONS - Notlardaki gereksinim

    public async Task<ApiResponseDto<BulkReportApprovalResponseDto>> BulkApproveReportsAsync(
        BulkReportApprovalRequestDto request, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = new BulkReportApprovalResponseDto
        {
            RequestedCount = request.Reports.Count,
            Message = $"Toplu onay işlemi başlatıldı - {request.Reports.Count} rapor"
        };

        foreach (var reportItem in request.Reports)
        {
            try
            {
                var approvalRequest = new ReportApprovalRequestDto
                {
                    Username = request.Username,
                    CompanyCode = request.CompanyCode,
                    WsPassword = request.WsPassword,
                    TcIdentityNumber = reportItem.TcIdentityNumber,
                    CaseType = reportItem.CaseType,
                    ReportId = reportItem.ReportId,
                    Status = reportItem.Status,
                    Date = reportItem.Date
                };

                var result = await ApproveReportAsync(approvalRequest, ct);

                if (result.Success && result.Data?.Success == true)
                {
                    response.SuccessList.Add(new ReportApprovalSuccessDto
                    {
                        ReportId = reportItem.ReportId,
                        TcIdentityNumber = reportItem.TcIdentityNumber,
                        EmployeeName = reportItem.EmployeeName ?? "",
                        CaseType = reportItem.CaseType,
                        Status = reportItem.Status
                    });
                }
                else
                {
                    response.ErrorList.Add(new ReportApprovalErrorDto
                    {
                        ReportId = reportItem.ReportId,
                        TcIdentityNumber = reportItem.TcIdentityNumber,
                        CaseType = reportItem.CaseType,
                        ErrorCode = result.Data?.ResultCode ?? -1,
                        ErrorMessage = result.Message
                    });
                }

                // Kısa bekleme
                await Task.Delay(100, ct);
            }
            catch (Exception ex)
            {
                response.ErrorList.Add(new ReportApprovalErrorDto
                {
                    ReportId = reportItem.ReportId,
                    TcIdentityNumber = reportItem.TcIdentityNumber,
                    CaseType = reportItem.CaseType,
                    ErrorCode = -1,
                    ErrorMessage = $"İşlem hatası: {ex.Message}"
                });
            }
        }

        stopwatch.Stop();
        response.Success = response.SuccessCount > 0;
        response.Message = $"Toplu onay tamamlandı - Başarılı: {response.SuccessCount}, Hata: {response.ErrorCount}, Süre: {stopwatch.Elapsed.TotalSeconds:F1}s";

        return new ApiResponseDto<BulkReportApprovalResponseDto>
        {
            Success = response.Success,
            Message = response.Message,
            Data = response
        };
    }

    public async Task<ApiResponseDto<BulkReportCancellationResponseDto>> BulkCancelReportApprovalsAsync(
        BulkReportCancellationRequestDto request, CancellationToken ct = default)
    {
        var response = new BulkReportCancellationResponseDto
        {
            RequestedCount = request.ReportIds.Count,
            Message = $"Toplu iptal işlemi başlatıldı - {request.ReportIds.Count} rapor"
        };

        foreach (var reportId in request.ReportIds)
        {
            try
            {
                var cancellationRequest = new ReportCancellationRequestDto
                {
                    Username = request.Username,
                    CompanyCode = request.CompanyCode,
                    WsPassword = request.WsPassword,
                    ReportId = reportId
                };

                var result = await CancelReportApprovalAsync(cancellationRequest, ct);

                if (result.Success && result.Data?.Success == true)
                {
                    response.SuccessList.Add(new ReportCancellationSuccessDto
                    {
                        ReportId = reportId
                    });
                }
                else
                {
                    response.ErrorList.Add(new ReportCancellationErrorDto
                    {
                        ReportId = reportId,
                        ErrorCode = result.Data?.ResultCode ?? -1,
                        ErrorMessage = result.Message
                    });
                }

                await Task.Delay(100, ct);
            }
            catch (Exception ex)
            {
                response.ErrorList.Add(new ReportCancellationErrorDto
                {
                    ReportId = reportId,
                    ErrorCode = -1,
                    ErrorMessage = $"İşlem hatası: {ex.Message}"
                });
            }
        }

        response.Success = response.SuccessCount > 0;
        response.Message = $"Toplu iptal tamamlandı - Başarılı: {response.SuccessCount}, Hata: {response.ErrorCount}";

        return new ApiResponseDto<BulkReportCancellationResponseDto>
        {
            Success = response.Success,
            Message = response.Message,
            Data = response
        };
    }

    public async Task<ApiResponseDto<BulkCloseReportResponseDto>> BulkCloseReportsAsync(
        BulkCloseReportRequestDto request, CancellationToken ct = default)
    {
        var response = new BulkCloseReportResponseDto
        {
            RequestedCount = request.ReportIds.Count,
            Message = $"Toplu kapatma işlemi başlatıldı - {request.ReportIds.Count} rapor"
        };

        foreach (var reportId in request.ReportIds)
        {
            try
            {
                var closeRequest = new CloseReportRequestDto
                {
                    Username = request.Username,
                    CompanyCode = request.CompanyCode,
                    WsPassword = request.WsPassword,
                    ReportId = reportId
                };

                var result = await CloseReportAsync(closeRequest, ct);

                if (result.Success && result.Data?.Success == true)
                {
                    response.SuccessList.Add(new CloseReportSuccessDto
                    {
                        ReportId = reportId
                    });
                }
                else
                {
                    response.ErrorList.Add(new CloseReportErrorDto
                    {
                        ReportId = reportId,
                        ErrorCode = result.Data?.ResultCode ?? -1,
                        ErrorMessage = result.Message
                    });
                }

                await Task.Delay(50, ct);
            }
            catch (Exception ex)
            {
                response.ErrorList.Add(new CloseReportErrorDto
                {
                    ReportId = reportId,
                    ErrorCode = -1,
                    ErrorMessage = $"İşlem hatası: {ex.Message}"
                });
            }
        }

        response.Success = response.SuccessCount > 0;
        response.Message = $"Toplu kapatma tamamlandı - Başarılı: {response.SuccessCount}, Hata: {response.ErrorCount}";

        return new ApiResponseDto<BulkCloseReportResponseDto>
        {
            Success = response.Success,
            Message = response.Message,
            Data = response
        };
    }

    // Diğer metodların implementasyonları...
    public async Task<ApiResponseDto<ComprehensiveReportResponseDto>> GetComprehensiveReportsAsync(
        ReportSearchRequestDto request, bool includeApprovedReports = true, CancellationToken ct = default)
    {
        // Implementation placeholder
        throw new NotImplementedException("Comprehensive reports will be implemented in next iteration");
    }

    public async Task<ApiResponseDto<bool>> ValidateCredentialsAsync(
        LoginRequestDto request, CancellationToken ct = default)
    {
        var loginResult = await LoginAsync(request, ct);
        return new ApiResponseDto<bool>
        {
            Success = loginResult.Success,
            Message = loginResult.Message,
            Data = loginResult.Success
        };
    }

    public async Task<ApiResponseDto<ReportStatisticsDto>> GetReportStatisticsAsync(
        ReportSearchRequestDto request, CancellationToken ct = default)
    {
        throw new NotImplementedException("Report statistics will be implemented in next iteration");
    }

    public async Task<ApiResponseDto<AutoProcessResultDto>> AutoProcessReportsAsync(
        ReportSearchRequestDto request, bool autoClose = false, CancellationToken ct = default)
    {
        throw new NotImplementedException("Auto process will be implemented in next iteration");
    }

    // Helper Methods
    private ReportItemDto MapToReportItem(RaporAramaTarihleBean bean)
    {
        return new ReportItemDto
        {
            TcIdentityNumber = bean.TCKIMLIKNO ?? "",
            FirstName = bean.AD ?? "",
            LastName = bean.SOYAD ?? "",
            ReportId = long.TryParse(bean.MEDULARAPORID, out var id) ? id : 0,
            ReportTrackingNumber = bean.RAPORTAKIPNO ?? "",
            ReportSequenceNumber = bean.RAPORSIRANO ?? "",
            ClinicDate = DateHelper.TryParseSgkDate(bean.POLIKLINIKTAR),
            InpatientStartDate = DateHelper.TryParseSgkDate(bean.YATRAPBASTAR),
            InpatientEndDate = DateHelper.TryParseSgkDate(bean.YATRAPBITTAR),
            OutpatientStartDate = DateHelper.TryParseSgkDate(bean.ABASTAR),
            OutpatientEndDate = DateHelper.TryParseSgkDate(bean.ABITTAR),
            WorkControlDate = DateHelper.TryParseSgkDate(bean.ISBASKONTTAR),
            CaseCode = bean.VAKA ?? "",
            CaseName = bean.VAKAADI ?? "",
            ReportStatus = bean.RAPORDURUMU ?? "",
            FacilityCode = bean.TESISKODU ?? "",
            FacilityName = bean.TESISADI ?? "",
            Archive = bean.ARSIV
        };
    }

    private ApprovedReportItemDto MapToApprovedReportItem(OnayliRaporlarTarihleBean bean)
    {
        return new ApprovedReportItemDto
        {
            TcIdentityNumber = bean.TCKIMLIKNO ?? "",
            FirstName = bean.AD ?? "",
            LastName = bean.SOYAD ?? "",
            ReportId = long.TryParse(bean.MEDULARAPORID, out var id) ? id : 0,
            ReportTrackingNumber = bean.RAPORTAKIPNO ?? "",
            ReportSequenceNumber = bean.RAPORSIRANO ?? "",
            ClinicDate = DateHelper.TryParseSgkDate(bean.POLIKLINIKTAR),
            WorkControlDate = DateHelper.TryParseSgkDate(bean.ISBASKONTTAR),
            AccidentDate = DateHelper.TryParseSgkDate(bean.ISKAZASITARIHI),
            CaseCode = bean.VAKA ?? "",
            CaseName = bean.VAKAADI ?? ""
        };
    }
}