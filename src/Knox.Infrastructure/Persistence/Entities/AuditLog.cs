namespace Knox.Infrastructure.Persistence.Entities;

public sealed class AuditLog
{
    public long Id { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? UserId { get; set; }
    public string EventType { get; set; } = default!;
    public string EntityName { get; set; } = default!;
    public string? EntityId { get; set; }
    public string? CorrelationId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DetailsJson { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
