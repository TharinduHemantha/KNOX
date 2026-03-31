namespace Knox.Application.Features.Projects.Common;

public sealed record ProjectDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string? Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record ProjectListItemDto(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt);

public sealed record ProjectSearchRequest(
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}
