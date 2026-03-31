# Entra ID token exchange + JIT provisioning implementation

This update adds a starter implementation for the dual-authentication model:

- local ASP.NET Core Identity login
- Microsoft Entra ID organizational sign-in
- API-issued JWT access tokens after Entra token validation
- just-in-time user provisioning into the local Identity store

## What was added

### API
- `POST /api/v1/auth/entra/exchange`
- Accepts an Entra ID token payload:

```json
{
  "idToken": "eyJ..."
}
```

### Application layer
- `IEntraTokenValidator`
- `EntraExchangeCommand`
- `EntraExchangeCommandHandler`
- `EntraExchangeCommandValidator`

### Infrastructure layer
- `EntraTokenValidator`
- updated `IdentityService` for JIT provisioning
- updated `ApplicationUser` with Entra linkage fields

## JIT provisioning rules in this starter

1. Validate the incoming Entra ID token against Entra metadata signing keys.
2. Require the configured Entra tenant id.
3. Resolve the application tenant from the request subdomain.
4. Look for an existing local user by:
   - `TenantId + EntraObjectId`
   - fallback to `TenantId + Email`
5. Reject sign-in when the email already belongs to a local-only account.
6. Create the local user when none exists.
7. Assign the configured default JIT role.
8. Issue the API's own JWT + refresh token.

## New user columns

The runtime model now expects these columns on `Users`:

- `EntraObjectId`
- `EntraTenantId`
- `ExternalSubject`
- `LastLoginAtUtc`

## Suggested SQL patch

```sql
ALTER TABLE [dbo].[Users] ADD [EntraObjectId] NVARCHAR(100) NULL;
ALTER TABLE [dbo].[Users] ADD [EntraTenantId] NVARCHAR(100) NULL;
ALTER TABLE [dbo].[Users] ADD [ExternalSubject] NVARCHAR(200) NULL;
ALTER TABLE [dbo].[Users] ADD [LastLoginAtUtc] DATETIMEOFFSET NULL;
GO
CREATE UNIQUE INDEX [IX_Users_TenantId_EntraObjectId]
    ON [dbo].[Users]([TenantId], [EntraObjectId])
    WHERE [EntraObjectId] IS NOT NULL;
GO
```

## Important implementation note

This is a starter implementation. It still needs local verification with the .NET 10 SDK because the current environment did not allow compile-time validation.

## Recommended next step

Add invitation-flow enforcement for Entra JIT users if you want to prevent first-time automatic tenant access without prior admin approval.
