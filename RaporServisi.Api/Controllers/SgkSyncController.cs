using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaporServisi.Application.Contracts;
using RaporServisi.Infrastructure.Persistence;
using RaporServisi.Domain.Entities;

namespace RaporServisi.Api.Controllers;

[ApiController]
[Route("api/v1/sgk")]
public class SgkSyncController : ControllerBase
{
    private readonly ISgkViziteClient _vizite;
    private readonly AppDbContext _db;

    public SgkSyncController(ISgkViziteClient vizite, AppDbContext db)
    {
        _vizite = vizite; _db = db;
    }

    [HttpPost("sync/{yyyyMMdd}")]
    public async Task<IActionResult> SyncByDate(string yyyyMMdd, CancellationToken ct)
    {
        if (!DateTime.TryParseExact(yyyyMMdd, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var dt))
            return BadRequest("Tarih formatı yyyyMMdd olmalı.");

        var items = await _vizite.GetReportsByDateAsync(dt, ct);

        int added = 0;
        foreach (var r in items)
        {
            var exists = await _db.SickReports.AnyAsync(x => x.SourceSystemId == r.MedulaRaporId.ToString(), ct);
            if (!exists)
            {
                _db.SickReports.Add(new SickReport
                {
                    Id = Guid.NewGuid(),
                    Tckn = r.Tckn,
                    SicilNo = "",
                    StartDate = r.PoliklinikTarihi ?? dt,
                    EndDate = r.PoliklinikTarihi ?? dt,
                    DiagnosisCode = r.VakaKodu?.ToString() ?? "",
                    SourceSystemId = r.MedulaRaporId.ToString(),
                    Status = "Imported",
                    CreatedAt = DateTime.UtcNow
                });
                added++;
            }
        }
        await _db.SaveChangesAsync(ct);

        return Ok(new { date = dt.ToString("yyyy-MM-dd"), fetched = items.Count, inserted = added });
    }

    [HttpPost("mark-read/{medulaRaporId:long}")]
    public async Task<IActionResult> MarkRead(long medulaRaporId, CancellationToken ct)
    {
        var ok = await _vizite.MarkReportAsReadAsync(medulaRaporId, ct);
        return Ok(new { medulaRaporId, ok });
    }
}
