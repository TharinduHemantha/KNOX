using Knox.Application.Abstractions.Security;
using Knox.Infrastructure.Logging.Writers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Knox.Infrastructure.Logging;

public static class AuditLogServiceCollectionExtensions
{
    public static IServiceCollection AddAuditLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuditLogOptions>(configuration.GetSection(AuditLogOptions.SectionName));

        var options = configuration.GetSection(AuditLogOptions.SectionName).Get<AuditLogOptions>() ?? new AuditLogOptions();

        switch (options.Type)
        {
            case AuditLogType.File:
                services.AddScoped<IAuditLogWriter, FileAuditLogWriter>();
                break;

            case AuditLogType.BlobStorage:
                services.AddScoped<IAuditLogWriter, BlobStorageAuditLogWriter>();
                break;

            case AuditLogType.ApplicationInsights:
                services.AddApplicationInsightsTelemetry(config =>
                {
                    config.ConnectionString = options.ApplicationInsights?.ConnectionString;
                });
                services.AddScoped<IAuditLogWriter, ApplicationInsightsAuditLogWriter>();
                break;

            case AuditLogType.Database:
            default:
                services.AddScoped<IAuditLogWriter, DatabaseAuditLogWriter>();
                break;
        }

        return services;
    }
}
