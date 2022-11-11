using Microsoft.AspNetCore.Mvc;

namespace GoalspireBackend.Controllers;

public class DashboardController : ApiBaseController
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult Get()
    {
        throw new NotImplementedException();
    }
}