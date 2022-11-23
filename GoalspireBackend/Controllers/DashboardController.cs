using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoalspireBackend.Controllers;

[ApiVersion("1")]
public class DashboardController : ApiBaseController
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public ActionResult Get()
    {
        throw new NotImplementedException();
    }
}