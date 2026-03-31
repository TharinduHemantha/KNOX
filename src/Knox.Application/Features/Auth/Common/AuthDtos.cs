namespace Knox.Application.Features.Auth.Common;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt);

public sealed record CreateEntraInvitationResponse(
    Guid InvitationId,
    string InvitationCode,
    DateTimeOffset ExpiresAt);
