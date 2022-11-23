using GoalspireBackend.Dto.Requests.Auth;
using GoalspireBackend.Dto.Response;
using GoalspireBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GoalspireBackend.Controllers;

[ApiVersion("1")]
public class AuthController : ApiBaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.Login(request);
        if (!response.Succeeded)
        {
            return Forbid();
        }

        return Ok(response);
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesErrorResponseType(typeof(ErrorResponse))]
    public async Task<ActionResult> Register([FromBody] RegisterRequest request)
    {
        var response = await _authService.Register(request);
        if (!response.Succeeded)
        {
            return BadRequest(new ErrorResponse(response.Errors.First().Description));
        }

        return Ok();
    }
    

    //[HttpPost("logout")]
    //[ProducesResponseType(StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
    //public async Task<ActionResult> Logout()
    //{
    //    await _authService.Logout();
    //    return Ok();
    //}
    
    [HttpGet("user-info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public ActionResult UserInfo()
    {
        throw new NotImplementedException();
    }
    
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public ActionResult ForgotPassword()
    {
        throw new NotImplementedException();
    }
    
    [HttpPost("confirm-email")]
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
    [Authorize]
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
