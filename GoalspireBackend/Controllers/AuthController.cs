using Microsoft.AspNetCore.Mvc;

namespace GoalspireBackend.Controllers;

[ApiVersion("1")]
public class AuthController : ApiBaseController
{
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult Login()
    {
        throw new NotImplementedException();
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult Register()
    {
        throw new NotImplementedException();
    }
    

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] //idk tbh, ale co vratit, kdyz tohle zavola nekdo zvenku se spatnym tokenem? Bud tohle, nebo 404
    public ActionResult Logout()
    {
        throw new NotImplementedException();
    }
    
    [HttpGet("user-info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult UserInfo()
    {
        throw new NotImplementedException();
    }
    
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult ForgotPassword()
    {
        throw new NotImplementedException();
    }
    
    [HttpGet("confirm-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult ConfirmEmail()
    {
        throw new NotImplementedException();
    }
    
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult ResetPassword()
    {
        throw new NotImplementedException();
    }
}

// [ApiVersion("2")]
// public class AuthController : ApiBaseController
// {
//     [HttpGet("confirm-email")]
//     public ActionResult ConfirmEmail()
//     {
//         throw new NotImplementedException();
//     }
// }