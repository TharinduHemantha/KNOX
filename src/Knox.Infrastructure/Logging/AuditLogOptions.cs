namespace Knox.Infrastructure.Logging;

public sealed class AuditLogOptions
{
    public const string SectionName = "AuditLog";

    public AuditLogType Type { get; set; } = AuditLogType.Database;

    public FileLogOptions? File { get; set; }

    public BlobStorageLogOptions? BlobStorage { get; set; }

    public ApplicationInsightsLogOptions? ApplicationInsights { get; set; }
}

public enum AuditLogType
{
    Database,
    File,
    BlobStorage,
    ApplicationInsights
}

public sealed class FileLogOptions
{
    public string Directory { get; set; } = "logs/audit";
    public string FileNameFormat { get; set; } = "audit-{0:yyyy-MM-dd}.json";
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10 MB
}

public sealed class BlobStorageLogOptions
{
    public string ConnectionString { get; set; } = default!;
    public string ContainerName { get; set; } = "audit-logs";
    public string BlobNameFormat { get; set; } = "{0:yyyy/MM/dd}/audit-{1}.json";
}

public sealed class ApplicationInsightsLogOptions
{
    public string ConnectionString { get; set; } = default!;
    public string EventName { get; set; } = "AuditLog";
}
