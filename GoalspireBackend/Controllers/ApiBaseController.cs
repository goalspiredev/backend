using Microsoft.AspNetCore.Mvc;

namespace GoalspireBackend.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class ApiBaseController : ControllerBase
{
    
}