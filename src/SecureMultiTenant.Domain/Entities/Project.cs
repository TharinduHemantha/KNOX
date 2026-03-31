using SecureMultiTenant.Domain.Common;
using SecureMultiTenant.Domain.Events;

namespace SecureMultiTenant.Domain.Entities;

public sealed class Project : BaseEntity, ITenantOwned
{
    private readonly List<IDomainEvent> _domainEvents = [];
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsArchived { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Project() { }

    public Project(Guid tenantId, string name, string code, string? description)
    {
        TenantId = tenantId;
        Name = name;
        Code = code;
        Description = description;
        _domainEvents.Add(new ProjectCreatedDomainEvent(Id, tenantId, name));
    }

    public void Update(string name, string? description, string? modifiedBy)
    {
        Name = name;
        Description = description;
        MarkModified(modifiedBy);
    }

    public void Archive(string? modifiedBy)
    {
        IsArchived = true;
        MarkModified(modifiedBy);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
