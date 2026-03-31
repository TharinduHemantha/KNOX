using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureMultiTenant.Application.Abstractions.Cqrs;
using SecureMultiTenant.Application.Features.Projects.Commands.CreateProject;
using SecureMultiTenant.Application.Features.Projects.Queries.GetProjectById;

namespace SecureMultiTenant.Api.Controllers;

[ApiVersion("1.0")]
[Authorize]
public sealed class ProjectsController(
    ICommandDispatcher commandDispatcher,
    IQueryDispatcher queryDispatcher) : BaseApiController
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
        var project = await queryDispatcher.DispatchAsync(new GetProjectByIdQuery(id), cancellationToken);
        return project is null ? NotFound() : Ok(project);
    }
}
