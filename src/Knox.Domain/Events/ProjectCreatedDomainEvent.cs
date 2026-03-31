using Knox.Domain.Common;

namespace Knox.Domain.Events;

public sealed record ProjectCreatedDomainEvent(Guid ProjectId, Guid TenantId, string Name) : IDomainEvent
{
    public DateTimeOffset OccurredOnUtc { get; } = DateTimeOffset.UtcNow;
}
