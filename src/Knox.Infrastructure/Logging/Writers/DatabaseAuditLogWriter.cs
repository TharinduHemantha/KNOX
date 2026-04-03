using Knox.Application.Abstractions.Security;
using Knox.Infrastructure.Persistence;
using Knox.Infrastructure.Persistence.Entities;

namespace Knox.Infrastructure.Logging.Writers;

public sealed class DatabaseAuditLogWriter(AppDbContext dbContext) : IAuditLogWriter
{
    public async Task WriteAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        dbContext.AuditLogs.Add(new AuditLog
        {
            TenantId = entry.TenantId,
            UserId = entry.UserId,
            EventType = entry.EventType,
            EntityName = entry.EntityName,
            EntityId = entry.EntityId,
            CorrelationId = entry.CorrelationId,
            IpAddress = entry.IpAddress,
            UserAgent = entry.UserAgent,
            DetailsJson = entry.DetailsJson,
            CreatedAtUtc = entry.CreatedAtUtc
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
