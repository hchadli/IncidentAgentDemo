namespace IncidentAgentDemo.Api.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IncidentDbContext db)
    {
        // Demo app: always recreate the database to pick up schema changes
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        var now = DateTime.UtcNow;

        db.Incidents.AddRange(
            new Incident
            {
                Id = 1,
                Title = "Payment gateway timeout in EU region",
                ServiceName = "Payments",
                Severity = "High",
                Status = "Open",
                CreatedAtUtc = now.AddHours(-3),
                Summary = "Stripe payment gateway returning 504 errors for EU customers. Retry logic is failing after 3 attempts. Approximately 12% of EU transactions affected."
            },
            new Incident
            {
                Id = 2,
                Title = "Payments reconciliation job stuck",
                ServiceName = "Payments",
                Severity = "Medium",
                Status = "Open",
                CreatedAtUtc = now.AddHours(-8),
                Summary = "Nightly reconciliation batch job has been running for 8 hours without progress. Suspected deadlock on the ledger table. No financial data loss confirmed yet."
            },
            new Incident
            {
                Id = 3,
                Title = "Duplicate charge reports from customers",
                ServiceName = "Payments",
                Severity = "High",
                Status = "Open",
                CreatedAtUtc = now.AddHours(-1),
                Summary = "Customer support received 47 reports of duplicate charges in the last hour. Root cause under investigation — may be related to the gateway timeout retries."
            },
            new Incident
            {
                Id = 4,
                Title = "OAuth token refresh failing intermittently",
                ServiceName = "Identity",
                Severity = "Medium",
                Status = "Open",
                CreatedAtUtc = now.AddHours(-5),
                Summary = "Approximately 5% of token refresh requests returning 401. Users experience session drops. The token cache appears to have stale entries."
            },
            new Incident
            {
                Id = 5,
                Title = "MFA SMS delivery delays",
                ServiceName = "Identity",
                Severity = "Low",
                Status = "Open",
                CreatedAtUtc = now.AddDays(-1),
                Summary = "SMS-based MFA codes are delayed by 30-60 seconds for Vodafone UK customers. Twilio reported carrier-level throttling. TOTP and push notifications unaffected."
            },
            new Incident
            {
                Id = 6,
                Title = "Push notification delivery rate dropped to 60%",
                ServiceName = "Notifications",
                Severity = "High",
                Status = "Open",
                CreatedAtUtc = now.AddHours(-2),
                Summary = "Firebase Cloud Messaging reports a 40% drop in delivery success rate. Android devices primarily affected. iOS via APNs remains stable."
            },
            new Incident
            {
                Id = 7,
                Title = "Email queue backlog exceeding SLA",
                ServiceName = "Notifications",
                Severity = "Medium",
                Status = "Open",
                CreatedAtUtc = now.AddHours(-6),
                Summary = "SendGrid email queue has 85,000 pending messages. Normal backlog is under 5,000. Rate limiting from SendGrid suspected due to bounce rate spike."
            },
            new Incident
            {
                Id = 8,
                Title = "Webhook retries exhausted for partner integrations",
                ServiceName = "Notifications",
                Severity = "Low",
                Status = "Resolved",
                CreatedAtUtc = now.AddDays(-2),
                Summary = "Partner webhook endpoints were unreachable for 4 hours. All retries exhausted. Manual replay completed. Root cause was partner firewall change."
            },
            new Incident
            {
                Id = 9,
                Title = "Identity service memory leak in production",
                ServiceName = "Identity",
                Severity = "High",
                Status = "Resolved",
                CreatedAtUtc = now.AddDays(-3),
                Summary = "Memory usage on identity pods growing linearly. Pods restarting every 4 hours. Fixed by patching the session serializer. Deployed in v2.14.3."
            }
        );

        db.ServiceHealths.AddRange(
            new ServiceHealth
            {
                Id = 1,
                ServiceName = "Payments",
                Status = "Degraded",
                LastCheckedAtUtc = now.AddMinutes(-5),
                Notes = "Gateway timeouts in EU region causing elevated error rates. Reconciliation job also stalled. Two High-severity incidents active."
            },
            new ServiceHealth
            {
                Id = 2,
                ServiceName = "Identity",
                Status = "Healthy",
                LastCheckedAtUtc = now.AddMinutes(-3),
                Notes = "Token refresh issue is intermittent and low-impact. MFA delays are carrier-specific. Overall service remains healthy."
            },
            new ServiceHealth
            {
                Id = 3,
                ServiceName = "Notifications",
                Status = "Degraded",
                LastCheckedAtUtc = now.AddMinutes(-7),
                Notes = "Push delivery rates significantly reduced. Email backlog growing. One High-severity incident active for push notifications."
            }
        );

        await db.SaveChangesAsync();
    }
}
