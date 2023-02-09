using GoalspireBackend.Data;
using GoalspireBackend.Models;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace GoalspireBackend.Services;

public class RemindingService : BackgroundService
{
    private readonly TimeSpan _remindersCheckPeriod;
    
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RemindingService> _logger;
    private readonly IConfiguration _configuration;

    public RemindingService(IServiceProvider serviceProvider, ILogger<RemindingService> logger, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;

        _remindersCheckPeriod = TimeSpan.FromSeconds(_configuration.GetValue<int>("Goals:Reminding:IntervalSeconds"));
    }

    bool ShouldRemindTask(Goal task)
    {
        if (task.Type != GoalType.Task) return false;
        
        var endDateTime = RoundDateDown(task.EndsAt, _remindersCheckPeriod);
        var currentDateTime = RoundDateDown(DateTime.UtcNow, _remindersCheckPeriod);
        
        return endDateTime.CompareTo(currentDateTime) == 0;
    }

    bool ShouldRemindGoal(Goal goal)
    {
        if (goal.Type != GoalType.Goal) return false;
        //this doesn't really work yet ↓ cause it checks only for tasks that are due the same few secs
        return RoundDateDown(goal.EndsAt, _remindersCheckPeriod).CompareTo(RoundDateDown(DateTime.UtcNow, _remindersCheckPeriod)) == 0;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer periodicTimer = new PeriodicTimer(_remindersCheckPeriod);
        while (!stoppingToken.IsCancellationRequested && await periodicTimer.WaitForNextTickAsync(stoppingToken)) // the actual code to be run each check time
        {
            try
            {
                // init all needed scoped services
                using IServiceScope scope = _serviceProvider.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                
                // pre-fetch the goals, settings and subscriptions now to avoid unneccessary db calls later
                var tasksAndGoals = await dataContext.Goals.Where(x => !x.IsCompleted).ToListAsync(stoppingToken);
                var tasksPending = tasksAndGoals.Where(ShouldRemindTask).ToList();
                var goalsPending = tasksAndGoals.Where(ShouldRemindGoal).ToList();

                var tasksUserIds = tasksPending.Select(x => x.UserId);
                var goalsUserIds = goalsPending.Select(x => x.UserId);

                var subscriptions = await dataContext.NotificationSubscriptions
                    .Where(x => tasksUserIds.Contains(x.UserId) || goalsUserIds.Contains(x.UserId))
                    .ToListAsync(stoppingToken);
                
                var userSettings = await dataContext.Settings
                    .Where(x => tasksUserIds.Contains(x.UserId) || goalsUserIds.Contains(x.UserId))
                    .ToListAsync(stoppingToken);
                
                try
                {
                    RemindAboutTasks(tasksPending, subscriptions, notificationService, dataContext); // doesn't need to be awaited, because we really don't want to wait for the notification to be pushed before going forward with the other tasks
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to remind about tasks: {Message}", ex.Message);
                }
                
                try
                {
                    RemindAboutGoals(goalsPending, subscriptions, userSettings, notificationService, dataContext); // doesn't need to be awaited, because we really don't want to wait for the notification to be pushed before going forward with the other tasks
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to remind about goals: {Message}", ex.Message);
                }
                
                
                //      ?? get a random goal and notify the user ?? //TODO:
            }
            catch (Exception ex)
            {
                _logger.LogError("Timer in background reminding service has failed: {Message}", ex.Message);
            }
        }
    }

    private async void RemindAboutTasks(List<Goal> tasks, List<NotificationSubscription> notificationSubscriptions, INotificationService notificationService, DataContext dataContext)
    {
        foreach (Goal task in tasks)
        {
            var subscriptions = notificationSubscriptions.Where(x => x.UserId == task.UserId);
            foreach (var sub in subscriptions)
            {
                await notificationService.Notify(new Dto.Requests.Notifications.SendNotificationRequest
                {
                    NotificationType = Dto.Requests.Notifications.NotificationType.Reminder,
                    Title = task.Title,
                    Message = task.Content,
                    UserId = task.UserId,
                    Id = sub.Id,
                    GoalId = task.Id
                });
            }
        }
    }

    private async void RemindAboutGoals(List<Goal> goals, List<NotificationSubscription> notificationSubscriptions, List<Settings> userSettings, INotificationService notificationService, DataContext dataContext)
    {
        foreach (Goal goal in goals)
        {
            var subscriptions = notificationSubscriptions.Where(x => x.UserId == goal.UserId);

            Settings? settings = userSettings.FirstOrDefault(x => x.UserId == goal.UserId);
            if (settings == null) return;
            
            //TODO: test out this code for timezones
            DateOnly date = DateOnly.FromDateTime(goal.EndsAt);
            DateTime notifDateTimeUnspec = date.ToDateTime(settings.DailyNotificationTime, DateTimeKind.Unspecified);
            DateTime utcNotifTime = TimeZoneInfo.ConvertTimeToUtc(notifDateTimeUnspec, settings.TimeZone);
            // this ↑ converts the user's preferred daily notification time for each goal to UTC, so we can check it
            
            // TODO: add "smart" reminding

            foreach(var sub in subscriptions)
            {
                await notificationService.Notify(new Dto.Requests.Notifications.SendNotificationRequest
                {
                    NotificationType = Dto.Requests.Notifications.NotificationType.Goal,
                    Id = sub.Id,
                    Message = goal.Content,
                    Title = goal.Title, // TODO: maybe set the text(s) to not only the text from the goal body, but something like "Hey, you set yoruself to complete this ... yada yada"
                    UserId = goal.UserId,
                    GoalId = goal.Id,
                });
            }
        }
    }

    private static DateTime RoundDateDown(DateTime dt, TimeSpan d)
    {
        return new DateTime((dt.Ticks / d.Ticks) * d.Ticks);
    }
}
