using Microsoft.AspNetCore.Mvc;

namespace GoalspireBackend.Controllers;

public class NewsletterController : ApiBaseController
{
    [HttpPost("subscribe")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult Subscribe()
    {
        throw new NotImplementedException();
    }
    
    [HttpPost("unsubscribe")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult Unsubscribe()
    {
        throw new NotImplementedException();
    }
}