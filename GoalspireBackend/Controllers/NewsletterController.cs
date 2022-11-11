using Microsoft.AspNetCore.Mvc;

namespace GoalspireBackend.Controllers;

public class NewsletterController : ApiBaseController
{
    [HttpPost("subscribe")]
    public ActionResult Subscribe()
    {
        throw new NotImplementedException();
    }
    
    [HttpPost("unsubscribe")]
    public ActionResult Unsubscribe()
    {
        throw new NotImplementedException();
    }
}