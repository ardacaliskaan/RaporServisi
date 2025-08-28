using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaporServisi.Domain.Entities;

public class SickReport
{
    public Guid Id { get; set; }
    public string Tckn { get; set; } = default!;
    public string SicilNo { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string DiagnosisCode { get; set; } = default!;
    public string SourceSystemId { get; set; } = default!;
    public string Status { get; set; } = "Imported";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
