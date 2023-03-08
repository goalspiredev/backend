using GoalspireBackend.Common;
using GoalspireBackend.Data;
using GoalspireBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace GoalspireBackend.Services;

public interface ISettingsService
{
    public Task<Settings> GetSettings();
    public Task<Result> ModifySettings(Settings settings);
}


public class SettingsService : ISettingsService
{
    private readonly DataContext _dataContext;
    private readonly ICurrentUserService _userService;

    public SettingsService(DataContext dataContext, ICurrentUserService currentUserService)
    {
        _dataContext = dataContext;
        _userService = currentUserService;
    }

    public async Task<Settings> GetSettings()
    {
        return await _dataContext.Settings.FirstAsync(x => x.UserId == _userService.UserId);
    }

    public async Task<Result> ModifySettings(Settings settings)
    {
        //TODO: maybe we should compare and save goal tags to the user's settings when he creates a new goal with said tags?

        Settings savedSettings = await GetSettings(); //TODO: this could be throwing erros,
                                                      //BUT it shouldn't happen, because user gets assigned default settings upon creation.
                                                      //But the DB needs to be flushed cause old users don't have settings
        
        savedSettings.DailyNotificationTime = settings.DailyNotificationTime;
        savedSettings.DefaultSnoozeDuration = settings.DefaultSnoozeDuration;
        savedSettings.DisableEmailNotifications = settings.DisableEmailNotifications;
        savedSettings.GoalTags = settings.GoalTags;
        savedSettings.IanaTimeZone = settings.IanaTimeZone;
        savedSettings.ReducedAnimations = settings.ReducedAnimations;
        savedSettings.TimeZone = settings.TimeZone;

        _dataContext.Settings.Update(savedSettings);
        await _dataContext.SaveChangesAsync();

        return Result.Success();
    }
}
