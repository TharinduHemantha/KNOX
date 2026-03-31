# Knox - Secure Multi-Tenant .NET 10 API Template

Enterprise-grade starter template for a **REST API** using:

- .NET 10 / C#
- Clean Architecture
- CQRS without MediatR
- ASP.NET Core Identity
- Microsoft Entra ID sign-in support
- Multi-tenancy via **subdomain**
- SQL Server
- EF Core for writes
- Dapper for reads
- Aggregate-specific repositories
- Explicit Unit of Work
- FluentValidation
- OWASP API Security Top 10 aligned controls

## Projects

- `src/Knox.Api`
- `src/Knox.Application`
- `src/Knox.Domain`
- `src/Knox.Infrastructure`
- `tests/Knox.UnitTests`
- `tests/Knox.IntegrationTests`
- `tests/Knox.AuthorizationTests`

## Important

This is a **starter skeleton / blueprint implementation**. It is designed to be extended and hardened for your environment.

## Suggested setup order

1. Create database
2. Scaffold Identity + business tables into Infrastructure
3. Configure connection string and Entra settings
4. Run the API
5. Implement command/query handlers incrementally
6. Add tests and threat-model review

See `docs/solution-starter-guide.md`.


## EF Core scaffolding alignment

This solution now includes separated EF Core entity configurations, a design-time DbContext factory, and infrastructure models for `RefreshTokens` and `AuditLogs`. See `docs/efcore-scaffolding-alignment.md`.
