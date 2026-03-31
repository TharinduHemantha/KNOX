using Microsoft.AspNetCore.Identity;

namespace Knox.Infrastructure.Identity;

public class ApplicationRole : IdentityRole<Guid>
{
    public Guid? TenantId { get; set; }
    public bool IsSystemRole { get; set; }
}
