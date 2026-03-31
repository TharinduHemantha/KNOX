using Microsoft.EntityFrameworkCore;

namespace Knox.Infrastructure.Persistence;

public partial class AppDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder builder)
    {
        // Reserved for database-first alignment customizations after a future scaffold refresh.
        // Keep manual overrides here so scaffold-safe files remain easier to review.
    }
}
