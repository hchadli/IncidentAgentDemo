using IncidentAgentDemo.Api.Data;
using IncidentAgentDemo.Contracts;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<IncidentDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Data Source=incidents.db"));

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseCors();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IncidentDbContext>();
    await SeedData.InitializeAsync(db);
}

// GET /incidents/open?serviceName=Payments
app.MapGet("/incidents/open", async (string? serviceName, IncidentDbContext db) =>
{
    var query = db.Incidents.Where(i => i.Status == "Open");

    if (!string.IsNullOrWhiteSpace(serviceName))
        query = query.Where(i => i.ServiceName == serviceName);

    var incidents = await query
        .OrderByDescending(i => i.CreatedAtUtc)
        .Select(i => new IncidentDto(
            i.Id, i.Title, i.ServiceName, i.Severity,
            i.Status, i.CreatedAtUtc, i.Summary,
            i.ClosedAtUtc, i.ResolutionNote))
        .ToListAsync();

    return Results.Ok(new OpenIncidentsResponse(
        serviceName ?? "All",
        incidents.Count,
        incidents));
});

// GET /incidents/{id}
app.MapGet("/incidents/{id:int}", async (int id, IncidentDbContext db) =>
{
    var incident = await db.Incidents.FindAsync(id);
    if (incident is null)
        return Results.NotFound(new { error = $"Incident {id} not found" });

    return Results.Ok(new IncidentDto(
        incident.Id, incident.Title, incident.ServiceName,
        incident.Severity, incident.Status, incident.CreatedAtUtc,
        incident.Summary, incident.ClosedAtUtc, incident.ResolutionNote));
});

// POST /incidents
app.MapPost("/incidents", async (CreateIncidentRequest request, IncidentDbContext db) =>
{
    var validSeverities = new[] { "Low", "Medium", "High", "Critical" };

    if (string.IsNullOrWhiteSpace(request.Title))
        return Results.BadRequest(new { error = "Title is required." });
    if (string.IsNullOrWhiteSpace(request.ServiceName))
        return Results.BadRequest(new { error = "ServiceName is required." });
    if (string.IsNullOrWhiteSpace(request.Severity))
        return Results.BadRequest(new { error = "Severity is required." });
    if (!validSeverities.Contains(request.Severity, StringComparer.OrdinalIgnoreCase))
        return Results.BadRequest(new { error = $"Severity must be one of: {string.Join(", ", validSeverities)}." });
    if (string.IsNullOrWhiteSpace(request.Summary))
        return Results.BadRequest(new { error = "Summary is required." });

    var incident = new Incident
    {
        Title = request.Title.Trim(),
        ServiceName = request.ServiceName.Trim(),
        Severity = request.Severity.Trim(),
        Status = "Open",
        CreatedAtUtc = DateTime.UtcNow,
        Summary = request.Summary.Trim()
    };

    db.Incidents.Add(incident);
    await db.SaveChangesAsync();

    var dto = new IncidentDto(
        incident.Id, incident.Title, incident.ServiceName,
        incident.Severity, incident.Status, incident.CreatedAtUtc,
        incident.Summary);

    return Results.Created($"/incidents/{incident.Id}", dto);
});

// POST /incidents/{id}/close
app.MapPost("/incidents/{id:int}/close", async (int id, CloseIncidentRequest? request, IncidentDbContext db) =>
{
    var incident = await db.Incidents.FindAsync(id);
    if (incident is null)
        return Results.NotFound(new { error = $"Incident {id} not found." });

    if (incident.Status is "Closed" or "Resolved")
        return Results.BadRequest(new { error = $"Incident {id} is already {incident.Status}." });

    incident.Status = "Closed";
    incident.ClosedAtUtc = DateTime.UtcNow;
    incident.ResolutionNote = request?.ResolutionNote?.Trim();

    await db.SaveChangesAsync();

    return Results.Ok(new IncidentDto(
        incident.Id, incident.Title, incident.ServiceName,
        incident.Severity, incident.Status, incident.CreatedAtUtc,
        incident.Summary, incident.ClosedAtUtc, incident.ResolutionNote));
});

// GET /services/{serviceName}/health
app.MapGet("/services/{serviceName}/health", async (string serviceName, IncidentDbContext db) =>
{
    var health = await db.ServiceHealths
        .FirstOrDefaultAsync(h => h.ServiceName == serviceName);

    if (health is null)
        return Results.NotFound(new { error = $"Service '{serviceName}' not found" });

    return Results.Ok(new ServiceHealthDto(
        health.Id, health.ServiceName, health.Status,
        health.LastCheckedAtUtc, health.Notes));
});

app.Run();

namespace IncidentAgentDemo.Api
{
    public partial class Program;
}
