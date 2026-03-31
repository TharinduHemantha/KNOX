namespace Knox.Domain.Common;

public interface ITenantOwned
{
    Guid TenantId { get; }
}
