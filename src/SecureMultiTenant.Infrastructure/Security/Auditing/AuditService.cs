using Microsoft.AspNetCore.Http;
using SecureMultiTenant.Application.Abstractions.Security;
using SecureMultiTenant.Infrastructure.Persistence;
using SecureMultiTenant.Infrastructure.Persistence.Entities;

namespace SecureMultiTenant.Infrastructure.Security.Auditing;

public sealed class AuditService(
    AppDbContext dbContext,
    IHttpContextAccessor httpContextAccessor,
    ICurrentUserService currentUserService,
    ITenantContext tenantContext) : IAuditService
{
    public async Task WriteSecurityEventAsync(AuditSecurityEvent auditEvent, CancellationToken cancellationToken = default)
    {
        Guid? auditUserId = auditEvent.UserId;
        if (auditUserId is null && Guid.TryParse(currentUserService.UserId, out var parsedUserId))
        {
            auditUserId = parsedUserId;
        }

        dbContext.AuditLogs.Add(new AuditLog
        {
            TenantId = auditEvent.TenantId ?? tenantContext.TenantId,
            UserId = auditUserId,
            EventType = auditEvent.EventType,
            EntityName = auditEvent.EntityName,
            EntityId = auditEvent.EntityId,
            CorrelationId = auditEvent.CorrelationId ?? httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString(),
            IpAddress = auditEvent.IpAddress ?? httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = auditEvent.UserAgent ?? httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString(),
            DetailsJson = auditEvent.DetailsJson,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
