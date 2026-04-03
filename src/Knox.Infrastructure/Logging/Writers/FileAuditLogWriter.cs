using System.Text.Json;
using Knox.Application.Abstractions.Security;
using Microsoft.Extensions.Options;

namespace Knox.Infrastructure.Logging.Writers;

public sealed class FileAuditLogWriter : IAuditLogWriter
{
    private readonly FileLogOptions _options;
    private readonly object _lock = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public FileAuditLogWriter(IOptions<AuditLogOptions> options)
    {
        _options = options.Value.File ?? new FileLogOptions();
    }

    public Task WriteAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        var directory = _options.Directory;
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var fileName = string.Format(_options.FileNameFormat, entry.CreatedAtUtc);
        var filePath = Path.Combine(directory, fileName);

        var logLine = JsonSerializer.Serialize(entry, JsonOptions) + Environment.NewLine;

        lock (_lock)
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists && fileInfo.Length >= _options.MaxFileSizeBytes)
            {
                var rotatedName = Path.Combine(directory, $"{Path.GetFileNameWithoutExtension(fileName)}-{DateTime.UtcNow:HHmmss}{Path.GetExtension(fileName)}");
                File.Move(filePath, rotatedName);
            }

            File.AppendAllText(filePath, logLine);
        }

        return Task.CompletedTask;
    }
}
