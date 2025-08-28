using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaporServisi.Infrastructure.Persistence;
using RaporServisi.Domain.Entities;

namespace RaporServisi.Api.Controllers;

[ApiController]
[Route("api/v1/reports")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ReportsController(AppDbContext db) => _db = db;

    // Raporlarý listele (opsiyonel filtreler)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> Get([FromQuery] string? tckn, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var q = _db.SickReports.AsQueryable();
        if (!string.IsNullOrWhiteSpace(tckn)) q = q.Where(x => x.Tckn == tckn);
        if (from.HasValue) q = q.Where(x => x.StartDate >= from.Value);
        if (to.HasValue) q = q.Where(x => x.EndDate <= to.Value);

        var list = await q
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new {
                x.Id,
                x.Tckn,
                x.SicilNo,
                x.StartDate,
                x.EndDate,
                x.DiagnosisCode,
                x.Status
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Seed([FromBody] SickReport model)
    {
        model.Id = Guid.NewGuid();
        model.CreatedAt = DateTime.UtcNow;
        _db.SickReports.Add(model);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
    }
}
