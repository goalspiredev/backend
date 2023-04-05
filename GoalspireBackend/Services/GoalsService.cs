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
    private readonly ISettingsService _settingsService;

    public GoalsService(ICurrentUserService userService, DataContext dataContext, ISettingsService settingsService)
    {
        _userService = userService;
        _dataContext = dataContext;
        _settingsService = settingsService;
    }

    public async Task<List<Goal>> GetAllGoals()
    {
        return await _dataContext.Goals
            .Where(x => x.UserId == _userService.UserId)
            .ToListAsync();
    }

    public async Task<Goal?> GetGoal(Guid id)
    {
        return await _dataContext.Goals.FirstOrDefaultAsync(x => (x.UserId == _userService.UserId) && x.Id == id);
    }

    public async Task<Goal> CreateGoal(Goal goal)
    {
        goal.Id = Guid.NewGuid();
        goal.UserId = _userService.UserId!.Value;
        goal.CreatedAt = DateTime.UtcNow;

        ValidateGoal(goal);

        _dataContext.Goals.Add(goal);
        await _dataContext.SaveChangesAsync();

        await _settingsService.CheckAndAddTags(goal.Tags);

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
        goal.IsCompleted = goalUpdate.IsCompleted;
        goal.Title = goalUpdate.Title;
        goal.EndsAt = goalUpdate.EndsAt;
        goal.Type = goalUpdate.Type;
        goal.Priority = goalUpdate.Priority;
        goal.Tags = goalUpdate.Tags;


        goal.UpdatedAt = DateTime.UtcNow;

        ValidateGoal(goal); // if it fails, it will throw exception and return 400

        _dataContext.Goals.Update(goal);
        await _dataContext.SaveChangesAsync();

        await _settingsService.CheckAndAddTags(goal.Tags);

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

        return Result.Success();
    }

    private void ValidateGoal(Goal goal)
    {
        if (goal.Title == string.Empty)
            throw new BadHttpRequestException("Title mustn't be empty!");
        //if(goal.Content == String.Empty) // I'd allow content to be empty, cause sometimes you just want a quick task with just the title
        if (goal.Tags.Any(t => t == string.Empty))
            throw new BadHttpRequestException("Tag name(s) mustn't be empty!");
        if (goal.Priority > Goal.MaxPriority)
            goal.Priority = Goal.MaxPriority;
    }
}