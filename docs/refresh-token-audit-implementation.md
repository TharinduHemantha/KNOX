# Refresh token persistence + audit logging implementation

This update adds starter-grade persistence and audit logging that align the runtime behavior with the SQL schema.

## What was added

### Refresh token persistence
- `IRefreshTokenService`
- `RefreshTokenService`
- login handler now persists a **hashed** refresh token
- refresh endpoint rotates refresh tokens
- revoke endpoint revokes a refresh token explicitly
- `JwtTokenService` now returns both the raw refresh token and its SHA-256 hash

### Audit logging
- `IAuditService`
- `AuditService`
- auth flows now write security audit events:
  - `auth.local.succeeded`
  - `auth.local.failed`
  - `auth.refresh.succeeded`
  - `auth.refresh.failed`
  - `auth.refresh.revoked`
- `AppDbContext.SaveChangesAsync` now:
  - stamps `CreatedBy`, `CreatedAtUtc`, `LastModifiedBy`, `LastModifiedAtUtc`
  - appends entity audit logs for tracked domain entities

## API endpoints added

- `POST /api/v1/auth/refresh`
- `POST /api/v1/auth/refresh/revoke`

## Important design notes

1. Refresh tokens are stored as hashes, never in plaintext.
2. Rotation revokes the current token and creates a new one.
3. Audit logging is starter-grade and intentionally simple.
4. The revoke endpoint currently requires a valid access token.
5. IP address and user agent capture are scaffolded through `HttpContext` access.

## Next recommended improvements

- add a refresh-token family chain and reuse-detection response
- add device fingerprint / session name support
- add audit-log querying endpoints for admins
- wire correlation ID, IP, and user agent into auth audit calls explicitly
- add integration tests for refresh token rotation and revocation
