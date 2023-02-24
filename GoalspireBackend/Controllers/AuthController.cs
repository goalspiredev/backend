using GoalspireBackend.Common;
using GoalspireBackend.Dto.Requests.Auth;
using GoalspireBackend.Dto.Response;
using GoalspireBackend.Dto.Response.Auth;
using GoalspireBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;

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
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.Login(request);
        if (!response.Succeeded)
        {
            return StatusCode(StatusCodes.Status403Forbidden, response.Note ?? "Name or password is invalid.");
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

    [HttpPost("confirm-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result>> ConfirmEmail(ConfirmEmailRequest request)
    {
        var result = await _authService.ConfirmEmail(request);
        if (result.Succeeded)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPassword(request);

        return Ok(); //OK regardless of the acutal result, so user can't find out if the account exists.
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result>> ResetPassword(ResetPasswordRequest request)
    {
        var result = await _authService.ResetPassword(request);

        if (result.Succeeded)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }
}

//  Example of how a v2 api would be implemented
// [ApiVersion("2")]
// public class AuthController : ApiBaseController
// {
//     [HttpGet("confirm-email")]
//     public ActionResult ConfirmEmail()
//     {
//         throw new NotImplementedException();
//     }
// }
