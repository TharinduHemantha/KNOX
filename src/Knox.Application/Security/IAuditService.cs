namespace Knox.Application.Abstractions.Security;

public interface IAuditService
{
    Task WriteSecurityEventAsync(AuditSecurityEvent auditEvent, CancellationToken cancellationToken = default);
}

public sealed record AuditSecurityEvent(
    string EventType,
    string EntityName,
    string? EntityId,
    string? DetailsJson,
    Guid? TenantId,
    Guid? UserId,
    string? CorrelationId,
    string? IpAddress,
    string? UserAgent);
