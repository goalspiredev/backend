using GoalspireBackend.Dto.Requests.Notifications;
using GoalspireBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebPush;

namespace GoalspireBackend.Controllers;

public class NotificationsTestController : ApiBaseController
{
    private readonly INotificationService _notificationService;
    public NotificationsTestController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }


    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Register([FromBody] NotificationSubscriptionRegisterRequest request)
    {
        var result = await _notificationService.Register(request);

        if (result.Succeeded)
        {
            return Ok();
        }
        return BadRequest(result.Error); // I think the only thing that can go wrong is invalid request - wront keys or whatnot
    }

    [HttpPost("notify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Notify(SendNotificationRequest request)
    {
        var result = await _notificationService.Notify(request);

        if (result.Succeeded)
        {
            return Ok();
        }

        return BadRequest(result.Error);
    }
}
