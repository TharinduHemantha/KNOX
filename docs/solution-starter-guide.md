# Solution Starter Guide

This guide explains the generated solution **folder by folder** and gives you the code-starting point for your secure API template.

## 1. Solution layout

```text
Knox/
├── README.md
├── docs/
│   └── solution-starter-guide.md
├── Knox.sln
├── src/
│   ├── Knox.Api/
│   ├── Knox.Application/
│   ├── Knox.Domain/
│   └── Knox.Infrastructure/
└── tests/
    ├── Knox.UnitTests/
    ├── Knox.IntegrationTests/
    └── Knox.AuthorizationTests/
```

## 2. Architecture overview

### Domain
Contains:
- business entities
- value objects
- domain events
- repository contracts
- tenant/security abstractions

### Application
Contains:
- CQRS contracts
- handlers
- DTOs
- validation
- interfaces used by infrastructure
- authorization requirements for use cases

### Infrastructure
Contains:
- EF Core DbContext
- ASP.NET Identity integration
- Dapper read repositories
- token generation
- tenant resolution support
- security services
- audit logging persistence

### API
Contains:
- controllers
- middleware
- DI configuration
- authentication/authorization setup
- problem details
- rate limiting
- security headers
- health checks

## 3. Multi-tenant model

Tenant is resolved from subdomain.

Example:
- `tenant1.api.com` → tenant key = `tenant1`

The middleware resolves tenant from host and stores it in `TenantContext`.
The tenant is then used by:
- command handlers
- repositories
- authorization
- user creation / sign-in rules

## 4. Authentication design

Two login modes are planned:

### Local login
- ASP.NET Core Identity user store
- username/password
- lockout
- MFA ready
- anti-enumeration response pattern
- refresh token issuance

### Microsoft Entra ID sign-in
- organization login only
- API validates external identity token
- system performs JIT provisioning if enabled
- API issues **its own JWT + refresh token** for uniform authorization

## 5. Authorization design

- role + permission claims
- tenant-scoped roles
- global roles such as `SuperAdmin`
- future custom roles supported
- endpoint, record-level, and field-level hooks

## 6. CQRS without MediatR

This template uses:
- `ICommand<TResponse>`
- `IQuery<TResponse>`
- dispatcher interfaces
- handler interfaces
- DI registration by scanning or explicit registration

You can later replace this with another mediator if required.

## 7. Repository strategy

Per your decision:
- EF Core for write model
- Dapper for read model
- aggregate-specific repositories
- explicit Unit of Work

## 8. Database-first guidance

Because you chose database-first:
- keep schema as the source of truth
- scaffold tables into Infrastructure
- extend partial classes instead of editing scaffolded files directly

Recommended scaffold areas:
- Identity / security tables
- tenants
- projects
- audit logs

## 9. First implementation order

1. Database schema
2. Scaffold DbContext + entities
3. Tenant resolution middleware
4. Identity setup
5. JWT/refresh token service
6. Entra sign-in exchange endpoint
7. Role/permission authorization service
8. Project CRUD CQRS flow
9. Tests
10. Hardening

## 10. How to scaffold from SQL Server

Example command, run on your machine after installing the EF Core CLI tools:

```bash
dotnet tool install --global dotnet-ef
dotnet ef dbcontext scaffold "Server=.;Database=KnoxDb;Trusted_Connection=True;TrustServerCertificate=True" Microsoft.EntityFrameworkCore.SqlServer   --project src/Knox.Infrastructure   --startup-project src/Knox.Api   --context AppDbContext   --output-dir Persistence/Scaffolded/Entities   --context-dir Persistence/Scaffolded   --use-database-names   --no-onconfiguring   --force
```

## 11. What to customize first

- `TenantResolutionOptions`
- `JwtOptions`
- `EntraOptions`
- `SecurityOptions`
- `ConnectionStrings`
- `Authorization:Permissions`
- database schema names
- lockout/MFA policy
- refresh token persistence

## 12. Warning

This starter is intentionally opinionated. Before production, review:
- key management
- token lifetimes
- CORS
- cookie settings if UI is added later
- audit retention
- PII logging policy
- SQL indexes for `TenantId`
