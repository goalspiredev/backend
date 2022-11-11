using Microsoft.AspNetCore.Mvc;

namespace GoalspireBackend.Controllers;

[ApiVersion("1")]
public class AuthController : ApiBaseController
{
    [HttpPost("login")]
    public ActionResult Login()
    {
        throw new NotImplementedException();
    }
    
    [HttpPost("register")]
    public ActionResult Register()
    {
        throw new NotImplementedException();
    }
    
    [HttpPost("logout")]
    public ActionResult Logout()
    {
        throw new NotImplementedException();
    }
    
    [HttpGet("user-info")]
    public ActionResult UserInfo()
    {
        throw new NotImplementedException();
    }
    
    [HttpPost("forgot-password")]
    public ActionResult ForgotPassword()
    {
        throw new NotImplementedException();
    }
    
    [HttpGet("confirm-email")]
    public ActionResult ConfirmEmail()
    {
        throw new NotImplementedException();
    }
    
    [HttpPost("reset-password")]
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