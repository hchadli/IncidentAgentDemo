using Microsoft.EntityFrameworkCore;

namespace IncidentAgentDemo.Api.Data;

public sealed class Incident
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string ServiceName { get; set; }
    public required string Severity { get; set; }
    public required string Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public required string Summary { get; set; }
}

public sealed class ServiceHealth
{
    public int Id { get; set; }
    public required string ServiceName { get; set; }
    public required string Status { get; set; }
    public DateTime LastCheckedAtUtc { get; set; }
    public required string Notes { get; set; }
}

public sealed class IncidentDbContext(DbContextOptions<IncidentDbContext> options) : DbContext(options)
{
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<ServiceHealth> ServiceHealths => Set<ServiceHealth>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Incident>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(256);
            e.Property(x => x.ServiceName).IsRequired().HasMaxLength(128);
            e.Property(x => x.Severity).IsRequired().HasMaxLength(32);
            e.Property(x => x.Status).IsRequired().HasMaxLength(32);
            e.Property(x => x.Summary).IsRequired().HasMaxLength(1024);
        });

        modelBuilder.Entity<ServiceHealth>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ServiceName).IsRequired().HasMaxLength(128);
            e.Property(x => x.Status).IsRequired().HasMaxLength(32);
            e.Property(x => x.Notes).IsRequired().HasMaxLength(512);
        });
    }
}
