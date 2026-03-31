using Microsoft.AspNetCore.Mvc;

namespace Knox.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class BaseApiController : ControllerBase
{
}
