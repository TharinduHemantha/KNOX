namespace SecureMultiTenant.Application.Features.Projects.Common;

public sealed record ProjectDto(Guid Id, Guid TenantId, string Name, string Code, string? Description, bool IsArchived);
public sealed record ProjectListItemDto(Guid Id, string Name, string Code, bool IsArchived);
public sealed record ProjectSearchRequest(int PageNumber = 1, int PageSize = 20, string? Search = null);
