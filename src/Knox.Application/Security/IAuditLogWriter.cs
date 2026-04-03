namespace Knox.Application.Abstractions.Security;

public interface IAuditLogWriter
{
    Task WriteAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
}

public sealed record AuditLogEntry(
    Guid? TenantId,
    Guid? UserId,
    string EventType,
    string EntityName,
    string? EntityId,
    string? CorrelationId,
    string? IpAddress,
    string? UserAgent,
    string? DetailsJson,
    DateTimeOffset CreatedAtUtc);
