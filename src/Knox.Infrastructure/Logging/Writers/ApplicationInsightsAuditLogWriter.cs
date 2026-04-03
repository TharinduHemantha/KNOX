using Knox.Application.Abstractions.Security;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Options;

namespace Knox.Infrastructure.Logging.Writers;

public sealed class ApplicationInsightsAuditLogWriter : IAuditLogWriter
{
    private readonly TelemetryClient _telemetryClient;
    private readonly ApplicationInsightsLogOptions _options;

    public ApplicationInsightsAuditLogWriter(TelemetryClient telemetryClient, IOptions<AuditLogOptions> options)
    {
        _telemetryClient = telemetryClient;
        _options = options.Value.ApplicationInsights ?? throw new InvalidOperationException("Application Insights options are not configured.");
    }

    public Task WriteAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        var eventTelemetry = new EventTelemetry(_options.EventName);

        if (entry.TenantId.HasValue)
            eventTelemetry.Properties["TenantId"] = entry.TenantId.Value.ToString();

        if (entry.UserId.HasValue)
            eventTelemetry.Properties["UserId"] = entry.UserId.Value.ToString();

        eventTelemetry.Properties["EventType"] = entry.EventType;
        eventTelemetry.Properties["EntityName"] = entry.EntityName;

        if (!string.IsNullOrEmpty(entry.EntityId))
            eventTelemetry.Properties["EntityId"] = entry.EntityId;

        if (!string.IsNullOrEmpty(entry.CorrelationId))
            eventTelemetry.Properties["CorrelationId"] = entry.CorrelationId;

        if (!string.IsNullOrEmpty(entry.IpAddress))
            eventTelemetry.Properties["IpAddress"] = entry.IpAddress;

        if (!string.IsNullOrEmpty(entry.UserAgent))
            eventTelemetry.Properties["UserAgent"] = entry.UserAgent;

        if (!string.IsNullOrEmpty(entry.DetailsJson))
            eventTelemetry.Properties["DetailsJson"] = entry.DetailsJson;

        eventTelemetry.Properties["CreatedAtUtc"] = entry.CreatedAtUtc.ToString("O");

        _telemetryClient.TrackEvent(eventTelemetry);

        return Task.CompletedTask;
    }
}
