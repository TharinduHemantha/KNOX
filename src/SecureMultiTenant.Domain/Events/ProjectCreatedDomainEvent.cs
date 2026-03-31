using SecureMultiTenant.Domain.Common;

namespace SecureMultiTenant.Domain.Events;

public sealed record ProjectCreatedDomainEvent(Guid ProjectId, Guid TenantId, string Name) : IDomainEvent
{
    public DateTimeOffset OccurredOnUtc { get; } = DateTimeOffset.UtcNow;
}
