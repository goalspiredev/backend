using GoalspireBackend.Data;
using GoalspireBackend.Models;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace GoalspireBackend.Services;

public class RemindingService : BackgroundService
{
    private readonly TimeSpan _remindersCheckPeriod = TimeSpan.FromSeconds(60); //TODO: in prod make it 1 minute    

    //private readonly DataContext _dataContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RemindingService> _logger;

    public RemindingService(/*DataContext dataContext, */IServiceProvider serviceProvider, ILogger<RemindingService> logger)
    {
        //_dataContext = dataContext;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer periodicTimer = new PeriodicTimer(_remindersCheckPeriod);
        while (!stoppingToken.IsCancellationRequested && await periodicTimer.WaitForNextTickAsync(stoppingToken)) // the actual code to be run each check time
        {
            try
            {
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    INotificationService notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    IAuthService authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                    DataContext dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();

                    var users = await authService.GetAllUsers();

                    foreach (User user in users)
                    {
                        try
                        {
                            var tasks = await dataContext.Goals.Where(x => x.Id == Guid.Parse(user.Id) && x.Type == GoalType.Task).ToListAsync(stoppingToken);
                            RemindAboutTasks(tasks, user, notificationService, dataContext); // doesn't need to be awaited, because we really don't want to wait for the notification to be pushed before going forward with the other tasks

                            var goals = await dataContext.Goals.Where(x => x.Id == Guid.Parse(user.Id) && x.Type == GoalType.Goal).ToListAsync(stoppingToken);
                            RemindAboutGoals(goals, user, notificationService, dataContext);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Timer in background reminidng servcice has failed for a user: {Message}", ex.Message);
                        }
                    }
                }

                //loop through all users

                //  get tasks that are due this minute
                //      remind the user about them all

                //  if the current time == daily notification time for the user
                //      get goals that are due today
                //      remind the user about them, in the order of their priority

                //      ?? get a random goal and notify the user ?? //TODO:
            }
            catch (Exception ex)
            {
                _logger.LogError("Timer in background reminidng servcice has failed: {Message}", ex.Message);
            }
        }
    }

    private async void RemindAboutTasks(List<Goal> tasks, User user, INotificationService notificationService, DataContext dataContext)
    {
        foreach (Goal task in tasks)
        {
            // compare the current time and the end time of the task with the precision of _remindersCheckPeriod (1min)
            if (RoundDateDown(DateTime.UtcNow, _remindersCheckPeriod).CompareTo(RoundDateDown(task.EndsAt, _remindersCheckPeriod)) == 0)
            {
                var subscriptions = await dataContext.NotificationSubscriptions.Where(x => x.UserId == task.UserId).ToListAsync();

                foreach (var sub in subscriptions)
                {
                    await notificationService.Notify(new Dto.Requests.Notifications.SendNotificationRequest
                    {
                        NotificationType = Dto.Requests.Notifications.NotificationType.Reminder,
                        Title = task.Title,
                        Message = task.Content,
                        UserId = Guid.Parse(user.Id),
                        Id = sub.Id
                    });
                }
            }
        }
    }

    private async void RemindAboutGoals(List<Goal> goals, User user, INotificationService notificationService, DataContext dataContext)
    {
        // TODO: this could maybe use some optimalization, so this method isn't called and the goals aren't even fetched from DB in the first place,
        //          when it's not the right time to remind the user about his goals. That should have use (60*24)-1 DB calls a day
        //          *per user*.
        //          But I'm not sure how to sneak the logic for checking the user's daily remind time in the parent method, cause it kinda belongs 
        //          in here and we're doing the time check the same way in the Task method too

        Settings? settings = dataContext.Settings.FirstOrDefault(x => x.UserId == Guid.Parse(user.Id));
        if (settings == null) return; // the user doesn't have settings (????)

        // get the user's notification subscription(s)
        var subscriptions = await dataContext.NotificationSubscriptions.Where(x => x.UserId == Guid.Parse(user.Id)).ToListAsync();

        foreach (Goal goal in goals)
        {
            // this *should* work
            //TODO: test out this code for timezones
            TimeZoneInfo timezone = settings.TimeZone;
            TimeOnly time = settings.DailyNotificationTime;
            DateOnly date = DateOnly.FromDateTime(goal.EndsAt);
            DateTime notifDateTimeUnspec = date.ToDateTime(time, DateTimeKind.Unspecified);
            DateTime utcNotifTime = TimeZoneInfo.ConvertTimeToUtc(notifDateTimeUnspec, timezone);
            // this up here ↑ converts the user's preferred daily notification time for each goal to UTC, so we can check it
            // thid down here ↓ checks if the calculated notification time is due within this run of the renimding timer
            if (RoundDateDown(utcNotifTime, _remindersCheckPeriod).CompareTo(RoundDateDown(DateTime.UtcNow, _remindersCheckPeriod)) != 0)
            {
                continue;
            }

            foreach(var sub in subscriptions)
            {
                await notificationService.Notify(new Dto.Requests.Notifications.SendNotificationRequest
                {
                    NotificationType = Dto.Requests.Notifications.NotificationType.Goal,
                    Id = sub.Id,
                    Message = goal.Content,
                    Title = goal.Title, // TODO: maybe set the text(s) to not only the text from the goal body, but something like "Hey, you set yoruself to complete this ... yada yada"
                    UserId = Guid.Parse(user.Id)
                });
            }
        }
    }

    private static DateTime RoundDateDown(DateTime dt, TimeSpan d)
    {
        return new DateTime((dt.Ticks / d.Ticks) * d.Ticks);
    }
}
