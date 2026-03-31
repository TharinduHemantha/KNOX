using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Knox.Application.Abstractions.Cqrs;
using Knox.Application.Abstractions.Security;
using Knox.Application.Features.Projects.Commands.CreateProject;
using Knox.Application.Features.Projects.Queries.GetProjectById;

namespace Knox.Api.Controllers;

[ApiVersion("1.0")]
[Authorize]
public sealed class ProjectsController(
    ICommandDispatcher commandDispatcher,
    IQueryDispatcher queryDispatcher,
    ITenantContext tenantContext) : BaseApiController
{
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var id = await commandDispatcher.DispatchAsync(request, cancellationToken);
        return Ok(id);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        if (!tenantContext.IsResolved || !tenantContext.TenantId.HasValue)
        {
            return BadRequest("Tenant not resolved.");
        }

        var project = await queryDispatcher.DispatchAsync(
            new GetProjectByIdQuery(tenantContext.TenantId.Value, id), 
            cancellationToken);
        return project is null ? NotFound() : Ok(project);
    }
}
