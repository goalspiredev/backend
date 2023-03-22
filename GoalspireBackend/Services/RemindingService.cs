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

    private const int BatchLoadSize = 100;

    public RemindingService(IServiceProvider serviceProvider, ILogger<RemindingService> logger, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        var configVal = configuration.GetValue<int>("Goals:Reminding:IntervalSeconds");
        if (configVal == 0) {
            configVal = 60;
        }
        _remindersCheckPeriod = TimeSpan.FromSeconds(configVal);
    }

    bool ShouldRemindTask(Goal task)
    {
        if (task.Type != GoalType.Task) return false;

        var prevRun = DateTime.UtcNow - _remindersCheckPeriod;
        var currRun = DateTime.UtcNow;

        return IsDateBetween(task.EndsAt, prevRun, currRun);
    }

    bool ShouldRemindGoal(Goal goal)
    {
        if (goal.Type != GoalType.Goal) return false;


        TimeSpan goalLength = goal.EndsAt - goal.CreatedAt;
        TimeSpan timeSinceCreate = DateTime.UtcNow - goal.CreatedAt;

        int defaultRemindIntervalDays = goalLength.TotalDays switch
        {
            (<= 7) => 1,
            (<= 31) => 3,
            (<= 90) => 7,
            _ => 14 //everything longer remind every two weeks
        };


        /*
         * urgent - 0       / 2
         * important - 1    / 1.5
         * medium - 2       / 1
         * small - 3        * 2
         */

        int remindIntervalDays = goal.Priority switch
        {
            0 => (int)Math.Ceiling((decimal)defaultRemindIntervalDays / 2),
            1 => (int)Math.Ceiling(defaultRemindIntervalDays / 1.5d),
            2 => defaultRemindIntervalDays,
            3 => defaultRemindIntervalDays * 2,
            _ => defaultRemindIntervalDays //shouldn't happen, but who knows...
        };




        if (timeSinceCreate.TotalDays == goalLength.TotalDays) // the goal ends today
        {
            return true;
        }


        return timeSinceCreate.TotalDays % remindIntervalDays == 0;
    }

    private async Task LoadReminderBatch(DataContext dataContext, INotificationService notificationService, CancellationToken stoppingToken, int skip = 0)
    {
        while (true)
        {
            _logger.LogDebug($"Loading batch of reminders, skip: {skip}");

            // pre-fetch the goals, settings and subscriptions now to avoid unnecessary db calls later
            var tasksAndGoals = await dataContext.Goals.Where(x => !x.IsCompleted)
                .OrderBy(x => x.CreatedAt)
                .Skip(skip)
                .Take(BatchLoadSize)
                .ToListAsync(stoppingToken);

            var tasksPending = tasksAndGoals.Where(ShouldRemindTask).ToList();
            var goalsPending = tasksAndGoals.Where(ShouldRemindGoal).ToList();

            _logger.LogDebug($"Fetched {0} task(s) and {1} goal(s) to be reminded", tasksPending.Count, goalsPending.Count);

            var tasksUserIds = tasksPending.Select(x => x.UserId);
            var goalsUserIds = goalsPending.Select(x => x.UserId);

            var subscriptions = await dataContext.NotificationSubscriptions.Where(x => tasksUserIds.Contains(x.UserId) || goalsUserIds.Contains(x.UserId))
                .ToListAsync(stoppingToken);

            var userSettings = await dataContext.Settings.Where(x => tasksUserIds.Contains(x.UserId) || goalsUserIds.Contains(x.UserId))
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

            if (tasksAndGoals.Count < BatchLoadSize)
            {
                skip += BatchLoadSize;
                continue;
            }

            break;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();

        using PeriodicTimer periodicTimer = new PeriodicTimer(_remindersCheckPeriod);
        while (!stoppingToken.IsCancellationRequested && await periodicTimer.WaitForNextTickAsync(stoppingToken)) // the actual code to be run each check time
        {
            try
            {
                _logger.LogDebug("Executing all pending reminders..");
                await LoadReminderBatch(dataContext, notificationService, stoppingToken);
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


            var prevRun = DateTime.UtcNow - _remindersCheckPeriod;
            var currRun = DateTime.UtcNow;

            if (!IsDateBetween(utcNotifTime, prevRun, currRun)) return; //if it's not the time of the day to remind the user abt his stuff


            foreach (var sub in subscriptions)
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

    private static bool IsDateBetween(DateTime dt, DateTime low, DateTime high)
    {
        return dt >= low && dt <= high;
    }

    private static DateTime RoundDateDown(DateTime dt, TimeSpan d)
    {
        return new DateTime((dt.Ticks / d.Ticks) * d.Ticks);
    }
}
