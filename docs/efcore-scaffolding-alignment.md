# EF Core entity configuration and scaffolding alignment

This update brings the starter solution closer to a **database-first friendly** setup while still preserving your chosen architecture:

- `IdentityDbContext<ApplicationUser, ApplicationRole, Guid>` remains the main runtime DbContext.
- entity mapping has been moved into separate `IEntityTypeConfiguration<T>` classes.
- a design-time `AppDbContextFactory` has been added for `dotnet ef` commands.
- scaffold-safe extension points have been added with `partial` DbContext support.
- infrastructure-only tables `RefreshTokens` and `AuditLogs` are now represented in code.

## Why this matters

Because your solution uses **ASP.NET Core Identity with custom user/role types**, scaffolding the entire database directly back into the main runtime DbContext is not the best approach. Identity already defines much of that model. Microsoft documents that reverse engineering generates entity classes and a DbContext from an existing schema, while Identity model customization is handled through your own EF model configuration. citeturn568360search0turn568360search1

The safe pattern for this solution is:

1. Keep `AppDbContext` as the handwritten runtime context.
2. Keep entity configuration in dedicated configuration classes.
3. Use scaffolding selectively for comparison or regeneration of database-first domain tables.
4. Review EF Core 10 breaking changes whenever you upgrade the SDK or packages. EF Core 10 requires the .NET 10 SDK/runtime. citeturn568360search2turn568360search11turn568360search8

## Files added or updated

### Updated
- `src/SecureMultiTenant.Infrastructure/Persistence/AppDbContext.cs`

### Added
- `src/SecureMultiTenant.Infrastructure/Persistence/AppDbContext.Partial.cs`
- `src/SecureMultiTenant.Infrastructure/Persistence/DesignTime/AppDbContextFactory.cs`
- `src/SecureMultiTenant.Infrastructure/Persistence/Entities/RefreshToken.cs`
- `src/SecureMultiTenant.Infrastructure/Persistence/Entities/AuditLog.cs`
- `src/SecureMultiTenant.Infrastructure/Persistence/Configurations/TenantConfiguration.cs`
- `src/SecureMultiTenant.Infrastructure/Persistence/Configurations/ProjectConfiguration.cs`
- `src/SecureMultiTenant.Infrastructure/Persistence/Configurations/ApplicationUserConfiguration.cs`
- `src/SecureMultiTenant.Infrastructure/Persistence/Configurations/ApplicationRoleConfiguration.cs`
- `src/SecureMultiTenant.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs`
- `src/SecureMultiTenant.Infrastructure/Persistence/Configurations/AuditLogConfiguration.cs`

## Resulting folder structure

```text
Persistence/
  AppDbContext.cs
  AppDbContext.Partial.cs
  DesignTime/
    AppDbContextFactory.cs
  Entities/
    AuditLog.cs
    RefreshToken.cs
  Configurations/
    ApplicationRoleConfiguration.cs
    ApplicationUserConfiguration.cs
    AuditLogConfiguration.cs
    ProjectConfiguration.cs
    RefreshTokenConfiguration.cs
    TenantConfiguration.cs
  Repositories/
    ProjectRepository.cs
    TenantRepository.cs
  UnitOfWork.cs
```

## Alignment rules for this project

### 1) Runtime context stays handwritten

Do **not** replace `AppDbContext` with a scaffolded DbContext.

Reason:
- the runtime context inherits from `IdentityDbContext<ApplicationUser, ApplicationRole, Guid>`
- table names are customized (`Users`, `Roles`)
- you have Clean Architecture conventions and security decisions that a full scaffold will not preserve

### 2) Keep scaffold-generated code isolated

If you need to compare the current SQL Server schema with your C# model, scaffold into a **temporary or comparison folder**, not over your runtime context.

Example CLI flow:

```bash
dotnet tool install --global dotnet-ef

dotnet ef dbcontext scaffold   "Server=YOUR_SQL_SERVER;Database=SecureMultiTenantDb;Trusted_Connection=True;TrustServerCertificate=True;"   Microsoft.EntityFrameworkCore.SqlServer   --project src/SecureMultiTenant.Infrastructure   --startup-project src/SecureMultiTenant.Api   --context ScaffoldComparisonDbContext   --context-dir Persistence/Scaffolding   --output-dir Persistence/Scaffolding/Entities   --namespace SecureMultiTenant.Infrastructure.Persistence.Scaffolding.Entities   --context-namespace SecureMultiTenant.Infrastructure.Persistence.Scaffolding   --table Tenants   --table Projects   --table RefreshTokens   --table AuditLogs   --data-annotations   --use-database-names   --force
```
```

Microsoft documents `dotnet ef dbcontext scaffold` as the CLI command for reverse engineering an existing database. citeturn568360search0turn568360search3

### 3) Do not scaffold Identity tables into the main model

Avoid scaffolding these into your runtime model unless you are intentionally building a separate comparison snapshot:
- `Users`
- `Roles`
- `AspNetUserClaims`
- `AspNetUserRoles`
- `AspNetRoleClaims`
- `AspNetUserLogins`
- `AspNetUserTokens`

Instead, maintain Identity alignment through:
- `ApplicationUser`
- `ApplicationRole`
- `ApplicationUserConfiguration`
- `ApplicationRoleConfiguration`
- `AppDbContext.OnModelCreating`

### 4) Put manual overrides in partial files

Use `AppDbContext.Partial.cs` for anything you want to preserve after comparing scaffold output.

Good uses:
- extra indexes
- query filters
- provider-specific tuning
- temporary compatibility workarounds after EF upgrades

### 5) Keep database names explicit

To match your SQL script exactly, important indexes and constraints are named in configuration where needed.

This reduces drift between:
- SQL script
- EF model
- future scaffolding comparisons

## Recommended next implementation step

Your SQL script already contains `RefreshTokens` and `AuditLogs`, and the EF model now knows about both tables. The next practical step is to wire them into application behavior:

1. persist refresh tokens in login/refresh flows
2. write audit log entries from middleware or a persistence interceptor
3. add integration tests for rowversion and soft delete behavior

## Quick verification checklist

After opening the solution locally:

```bash
dotnet restore
dotnet build
```

Then verify:
- `AppDbContext` sees `Tenants`, `Projects`, `RefreshTokens`, and `AuditLogs`
- `dotnet ef dbcontext info --project src/SecureMultiTenant.Infrastructure --startup-project src/SecureMultiTenant.Api`
- no duplicate entity mappings exist for Identity tables
- table names still resolve to `Users` and `Roles`

## Important note

I could update the starter source structure here, but I could not compile-test it in this environment because the .NET SDK is not installed in the container.
