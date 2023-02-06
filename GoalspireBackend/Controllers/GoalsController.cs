using GoalspireBackend.Common;
using GoalspireBackend.Data;
using GoalspireBackend.Models;
using GoalspireBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoalspireBackend.Controllers;

[ApiVersion("1")]
public class GoalsController : ApiBaseController
{
    private readonly IGoalsService _goalService;

    public GoalsController(IGoalsService goalService)
    {
        _goalService = goalService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public async Task<ActionResult<List<Goal>>> GetAllGoals()
    {
        return await _goalService.GetAllGoals();
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Goal?>> GetGoal([FromRoute] Guid id)
    {
        var goal = await _goalService.GetGoal(id);
        if (goal == null)
        {
            return NotFound();
        }
        return goal;        
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public async Task<ActionResult<Goal>> CreateGoal([FromBody] Goal goal)
    {
        return await _goalService.CreateGoal(goal);
    }
    
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<ActionResult<Goal>> UpdateGoal([FromRoute] Guid id, [FromBody] Goal goal)
    {
        return await _goalService.UpdateGoal(id, goal);
    }
    
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<ActionResult<Result>> DeleteGoal([FromRoute] Guid id)
    {
        return await _goalService.DeleteGoal(id);
    }
}