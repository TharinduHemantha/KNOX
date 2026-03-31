using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SecureMultiTenant.Application;
using SecureMultiTenant.Application.Abstractions.ReadModels;
using SecureMultiTenant.Application.Features.Projects.Common;

namespace SecureMultiTenant.Infrastructure.ReadModels;

public sealed class ProjectReadRepository(IConfiguration configuration) : IProjectReadRepository
{
    private string ConnectionString => configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection is missing.");

    public async Task<ProjectDto?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT TOP 1 Id, TenantId, Name, Code, Description, IsArchived
            FROM Projects
            WHERE TenantId = @TenantId AND Id = @Id AND IsDeleted = 0;
            """;

        await using var connection = new SqlConnection(ConnectionString);
        return await connection.QuerySingleOrDefaultAsync<ProjectDto>(new CommandDefinition(sql, new { TenantId = tenantId, Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<PagedResult<ProjectListItemDto>> SearchAsync(Guid tenantId, ProjectSearchRequest request, CancellationToken cancellationToken = default)
    {
        const string sql = """
            WITH Filtered AS
            (
                SELECT Id, Name, Code, IsArchived
                FROM Projects
                WHERE TenantId = @TenantId
                  AND IsDeleted = 0
                  AND (@Search IS NULL OR Name LIKE '%' + @Search + '%' OR Code LIKE '%' + @Search + '%')
            )
            SELECT Id, Name, Code, IsArchived
            FROM Filtered
            ORDER BY Name
            OFFSET (@Offset) ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(1)
            FROM Projects
            WHERE TenantId = @TenantId
              AND IsDeleted = 0
              AND (@Search IS NULL OR Name LIKE '%' + @Search + '%' OR Code LIKE '%' + @Search + '%');
            """;

        await using var connection = new SqlConnection(ConnectionString);
        using var grid = await connection.QueryMultipleAsync(new CommandDefinition(sql, new
        {
            TenantId = tenantId,
            request.Search,
            Offset = (request.PageNumber - 1) * request.PageSize,
            request.PageSize
        }, cancellationToken: cancellationToken));

        var items = (await grid.ReadAsync<ProjectListItemDto>()).ToList();
        var total = await grid.ReadSingleAsync<int>();

        return new PagedResult<ProjectListItemDto>(items, request.PageNumber, request.PageSize, total);
    }
}
