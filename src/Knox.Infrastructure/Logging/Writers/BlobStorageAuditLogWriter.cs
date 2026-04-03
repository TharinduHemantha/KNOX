using System.Text;
using System.Text.Json;
using Azure.Storage.Blobs;
using Knox.Application.Abstractions.Security;
using Microsoft.Extensions.Options;

namespace Knox.Infrastructure.Logging.Writers;

public sealed class BlobStorageAuditLogWriter : IAuditLogWriter
{
    private readonly BlobContainerClient _containerClient;
    private readonly BlobStorageLogOptions _options;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public BlobStorageAuditLogWriter(IOptions<AuditLogOptions> options)
    {
        _options = options.Value.BlobStorage ?? throw new InvalidOperationException("Blob storage options are not configured.");
        var blobServiceClient = new BlobServiceClient(_options.ConnectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(_options.ContainerName);
    }

    public async Task WriteAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        await _containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blobName = string.Format(_options.BlobNameFormat, entry.CreatedAtUtc, Guid.NewGuid().ToString("N")[..8]);
        var blobClient = _containerClient.GetBlobClient(blobName);

        var json = JsonSerializer.Serialize(entry, JsonOptions);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        await blobClient.UploadAsync(stream, overwrite: false, cancellationToken);
    }
}
