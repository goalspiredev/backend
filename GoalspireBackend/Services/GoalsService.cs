using GoalspireBackend.Common;
using GoalspireBackend.Data;
using GoalspireBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace GoalspireBackend.Services;

public interface IGoalsService
{
    Task<List<Goal>> GetAllGoals();
    Task<Goal?> GetGoal(Guid id);
    Task<Goal> CreateGoal(Goal goal);
    Task<Goal> UpdateGoal(Guid id, Goal goal);
    Task<Result> DeleteGoal(Guid id);
}

public class GoalsService : IGoalsService
{
    private readonly ICurrentUserService _userService;
    private readonly DataContext _dataContext;

    public GoalsService(ICurrentUserService userService, DataContext dataContext)
    {
        _userService = userService;
        _dataContext = dataContext;
    }

    public async Task<List<Goal>> GetAllGoals()
    {
        return await _dataContext.Goals
            .Where(x => x.UserId == _userService.UserId)
            .ToListAsync();
    }

    public async Task<Goal?> GetGoal(Guid id)
    {
        return await _dataContext.Goals.FirstOrDefaultAsync(x => (x.UserId == _userService.UserId || x.IsPublic) && x.Id == id);
    }

    public async Task<Goal> CreateGoal(Goal goal)
    {
        goal.Id = Guid.NewGuid();
        goal.UserId = _userService.UserId!.Value;
        goal.CreatedAt = DateTime.UtcNow;

        _dataContext.Goals.Add(goal);
        await _dataContext.SaveChangesAsync();

        return goal;
    }

    public async Task<Goal> UpdateGoal(Guid id, Goal goalUpdate)
    {
        var goal = await GetGoal(id);
        if (goal == null)
        {
            throw new BadHttpRequestException("Goal does not exist!");
        }
        
        if (goal.UserId != _userService.UserId) throw new BadHttpRequestException("You can't update this goal!", 403);

        goal.Content = goalUpdate.Content;
        goal.Priority = goalUpdate.Priority;
        goal.IsPublic = goalUpdate.IsPublic;
        goal.IsCompleted = goalUpdate.IsCompleted;
        goal.Title = goalUpdate.Title;
        goal.EndsAt = goalUpdate.EndsAt;
        goal.Type = goalUpdate.Type;
        goal.Priority = goalUpdate.Priority;
        
        goal.UpdatedAt = DateTime.UtcNow;

        _dataContext.Goals.Update(goal);
        await _dataContext.SaveChangesAsync();

        return goal;
    }

    public async Task<Result> DeleteGoal(Guid id)
    {
        var goal = await GetGoal(id);
        if (goal == null)
        {
            throw new BadHttpRequestException("Invalid goal ID supplied!");
        }

        if (goal.UserId != _userService.UserId) throw new BadHttpRequestException("You can't delete this goal!", 403);

        _dataContext.Goals.Remove(goal);
        await _dataContext.SaveChangesAsync();

        return Result.Success;
    }
}