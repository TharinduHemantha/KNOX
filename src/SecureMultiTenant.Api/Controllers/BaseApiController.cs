using Microsoft.AspNetCore.Mvc;

namespace SecureMultiTenant.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class BaseApiController : ControllerBase
{
}
