using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaporServisi.Application.Contracts;
using RaporServisi.Domain.Entities;
using RaporServisi.Infrastructure.External;
using RaporServisi.Infrastructure.Persistence;

namespace RaporServisi.Infrastructure.Services;

public class ReportSyncService : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<ReportSyncService> _log;
    private readonly SgkViziteOptions _opt;

    public ReportSyncService(IServiceProvider sp, ILogger<ReportSyncService> log, IOptions<SgkViziteOptions> opt)
    {
        _sp = sp; _log = log; _opt = opt.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var vizite = scope.ServiceProvider.GetRequiredService<ISgkViziteClient>();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var from = DateTime.Today.AddDays(-_opt.GunGeri);
                var to = DateTime.Today;

                for (var d = from; d <= to; d = d.AddDays(1))
                {
                    var reports = await vizite.GetReportsByDateAsync(d, stoppingToken);

                    foreach (var r in reports)
                    {
                        var exists = await db.SickReports.AnyAsync(x => x.SourceSystemId == r.MedulaRaporId.ToString(), stoppingToken);
                        if (!exists)
                        {
                            db.SickReports.Add(new SickReport
                            {
                                Id = Guid.NewGuid(),
                                Tckn = r.Tckn,
                                SicilNo = "", 
                                StartDate = r.PoliklinikTarihi ?? d,
                                EndDate = r.PoliklinikTarihi ?? d,
                                DiagnosisCode = r.VakaKodu?.ToString() ?? "",
                                SourceSystemId = r.MedulaRaporId.ToString(),
                                Status = "Imported",
                                CreatedAt = DateTime.UtcNow
                            });

                            if (_opt.OtoOkunduKapat)
                                _ = vizite.MarkReportAsReadAsync(r.MedulaRaporId, stoppingToken);
                        }
                    }

                    await db.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "SGK WS-Vizite senkronizasyon hatası");
            }

            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }
}

