using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Knox.Application.Abstractions.Security;
using Knox.Domain.Common;
using Knox.Domain.Entities;
using Knox.Infrastructure.Identity;
using Knox.Infrastructure.Persistence.Entities;

namespace Knox.Infrastructure.Persistence;

public partial class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private readonly ICurrentUserService? _currentUserService;
    private readonly ITenantContext? _tenantContext;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUserService? currentUserService = null,
        ITenantContext? tenantContext = null,
        IHttpContextAccessor? httpContextAccessor = null) : base(options)
    {
        _currentUserService = currentUserService;
        _tenantContext = tenantContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<EntraInvitation> EntraInvitations => Set<EntraInvitation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        OnModelCreatingPartial(builder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditMetadata();
        AppendEntityAuditLogs();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditMetadata()
    {
        var userId = _currentUserService?.UserId;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.MarkCreated(userId);
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.MarkModified(userId);
            }
        }
    }

    private void AppendEntityAuditLogs()
    {
        var trackedEntries = ChangeTracker.Entries<BaseEntity>()
            .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        if (trackedEntries.Count == 0)
        {
            return;
        }

        var correlationId = _httpContextAccessor?.HttpContext?.Items["CorrelationId"]?.ToString();
        var ipAddress = _httpContextAccessor?.HttpContext?.Connection.RemoteIpAddress?.ToString();
        var userAgent = _httpContextAccessor?.HttpContext?.Request.Headers.UserAgent.ToString();
        Guid? auditUserId = Guid.TryParse(_currentUserService?.UserId, out var parsedUserId) ? parsedUserId : null;

        foreach (var entry in trackedEntries)
        {
            var propertyChanges = entry.Properties
                .Where(p => p.IsModified || entry.State == EntityState.Added)
                .ToDictionary(
                    p => p.Metadata.Name,
                    p => new
                    {
                        Original = entry.State == EntityState.Added ? null : p.OriginalValue,
                        Current = p.CurrentValue
                    });

            Guid? tenantId = entry.Entity is ITenantOwned tenantOwned
                ? tenantOwned.TenantId
                : _tenantContext?.TenantId;

            AuditLogs.Add(new AuditLog
            {
                TenantId = tenantId,
                UserId = auditUserId,
                EventType = $"entity.{entry.State.ToString().ToLowerInvariant()}",
                EntityName = entry.Metadata.ClrType.Name,
                EntityId = entry.Entity.Id.ToString(),
                CorrelationId = correlationId,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                DetailsJson = JsonSerializer.Serialize(propertyChanges),
                CreatedAtUtc = DateTimeOffset.UtcNow
            });
        }
    }

    partial void OnModelCreatingPartial(ModelBuilder builder);
}
