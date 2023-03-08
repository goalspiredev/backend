using GoalspireBackend.Models;
using GoalspireBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoalspireBackend.Controllers;

[ApiVersion("1")]
public class SettingsController : ApiBaseController
{
    private readonly ISettingsService _settingsService;

    public SettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public async Task<ActionResult<Settings>> Get()
    {
        return await _settingsService.GetSettings();
    }
    
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public async Task<ActionResult> Modify(Settings settings)
    {
        var result = await _settingsService.ModifySettings(settings);
        if (!result.Succeeded)
        {
            return BadRequest(result.Error);
        }
        return Ok();

        throw new NotImplementedException();
    }
}