namespace SecureMultiTenant.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAtUtc { get; protected set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; protected set; }
    public DateTimeOffset? LastModifiedAtUtc { get; protected set; }
    public string? LastModifiedBy { get; protected set; }
    public bool IsDeleted { get; protected set; }
    public byte[] RowVersion { get; protected set; } = Array.Empty<byte>();

    public void MarkCreated(string? userId)
    {
        CreatedBy = userId;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void MarkModified(string? userId)
    {
        LastModifiedBy = userId;
        LastModifiedAtUtc = DateTimeOffset.UtcNow;
    }

    public void SoftDelete(string? userId)
    {
        IsDeleted = true;
        MarkModified(userId);
    }
}
