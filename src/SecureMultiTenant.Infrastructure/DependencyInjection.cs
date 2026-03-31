using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SecureMultiTenant.Application.Abstractions.Persistence;
using SecureMultiTenant.Application.Abstractions.ReadModels;
using SecureMultiTenant.Application.Abstractions.Security;
using SecureMultiTenant.Domain.Repositories;
using SecureMultiTenant.Infrastructure.Identity;
using SecureMultiTenant.Infrastructure.Persistence;
using SecureMultiTenant.Infrastructure.Persistence.Repositories;
using SecureMultiTenant.Infrastructure.ReadModels;
using SecureMultiTenant.Infrastructure.Security;
using SecureMultiTenant.Infrastructure.Security.Auditing;
using SecureMultiTenant.Infrastructure.Security.Identity;
using SecureMultiTenant.Infrastructure.Security.Tokens;

namespace SecureMultiTenant.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<EntraOptions>(configuration.GetSection(EntraOptions.SectionName));

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 12;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddRoles<ApplicationRole>()
            .AddSignInManager()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IProjectReadRepository, ProjectReadRepository>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IEntraTokenValidator, EntraTokenValidator>();
        services.AddScoped<IEntraInvitationService, EntraInvitationService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IAuditService, AuditService>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<TenantContextAccessor>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContextAccessor>());

        return services;
    }
}
