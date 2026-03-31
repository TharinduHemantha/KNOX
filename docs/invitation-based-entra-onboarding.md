# Invitation-based Entra onboarding

This update adds a starter implementation for controlled first-time Entra onboarding.

## Flow

1. A tenant admin or super admin calls `POST /api/v1/auth/entra/invitations`.
2. The API stores an `EntraInvitations` row for the tenant and returns an `InvitationCode`.
3. The invited user signs in with Microsoft Entra ID against the tenant subdomain.
4. `POST /api/v1/auth/entra/exchange` validates the Entra token.
5. If the user does not already exist locally, the API requires a matching, unexpired invitation by tenant + email.
6. The invitation is consumed and the local Identity user is created.
7. The API issues its own local JWT + refresh token.

## Starter notes

- This starter matches by `TenantId + Email` for first-time onboarding.
- If `EntraTenantId` was stored on the invitation, it must match the Entra token tenant.
- The invitation-specific role is used when present; otherwise the default JIT role is assigned.
- The current implementation returns an invitation code for administrative delivery. Email sending is intentionally left out of the starter.

## Endpoint

### Create invitation

`POST /api/v1/auth/entra/invitations`

```json
{
  "email": "new.user@contoso.com",
  "entraTenantId": "00000000-0000-0000-0000-000000000000",
  "roleName": "ApplicationUser",
  "expiryDays": 7
}
```

Response:

```json
{
  "invitationId": "00000000-0000-0000-0000-000000000000",
  "email": "new.user@contoso.com",
  "roleName": "ApplicationUser",
  "expiresAtUtc": "2026-03-31T12:00:00+00:00",
  "invitationCode": "HEXCODE..."
}
```

## Configuration

`Authentication:Entra:RequireInvitationForJitProvisioning = true` forces invitations for first-time Entra user creation. Existing linked users can still sign in normally.
