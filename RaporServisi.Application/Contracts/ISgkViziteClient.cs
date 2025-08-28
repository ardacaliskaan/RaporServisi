using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaporServisi.Application.Contracts;

public interface ISgkViziteClient
{
    Task<string> LoginAsync(CancellationToken ct);
    Task<IReadOnlyList<ViziteReportSummary>> GetReportsByDateAsync(DateTime tarih, CancellationToken ct);
    Task<bool> MarkReportAsReadAsync(long medulaRaporId, CancellationToken ct);
}

public record ViziteReportSummary(
    long MedulaRaporId,
    string Tckn,
    DateTime? PoliklinikTarihi,
    int? VakaKodu,
    string? VakaAdi);

