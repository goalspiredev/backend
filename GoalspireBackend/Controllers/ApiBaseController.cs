using Microsoft.AspNetCore.Mvc;

namespace GoalspireBackend.Controllers;

[ApiController]
[Route("v{version:apiVersion}/[controller]")]
public class ApiBaseController : ControllerBase
{
    
}