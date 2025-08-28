using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaporServisi.Infrastructure.External;

public class SgkViziteOptions
{
    public string Endpoint { get; set; } = default!;
    public string KullaniciAdi { get; set; } = default!;
    public string IsyeriKodu { get; set; } = default!;
    public string WsSifre { get; set; } = default!;
    public int GunGeri { get; set; } = 7;
    public bool OtoOkunduKapat { get; set; } = true;
}
