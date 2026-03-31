using SecureMultiTenant.Domain.Common;

namespace SecureMultiTenant.Domain.Entities;

public sealed class Tenant : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string Subdomain { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private Tenant() { }

    public Tenant(string name, string subdomain)
    {
        Name = name;
        Subdomain = subdomain;
    }

    public void Deactivate(string? modifiedBy)
    {
        IsActive = false;
        MarkModified(modifiedBy);
    }
}
