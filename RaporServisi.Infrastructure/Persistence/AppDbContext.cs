using Microsoft.EntityFrameworkCore;
using RaporServisi.Domain.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace RaporServisi.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) { }
    public DbSet<SickReport> SickReports => Set<SickReport>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<SickReport>().HasIndex(x => new { x.Tckn, x.StartDate, x.EndDate });
    }
}
