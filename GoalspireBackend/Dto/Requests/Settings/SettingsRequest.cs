namespace GoalspireBackend.Dto.Requests.Settings;

public class SettingsRequest
{
    public Guid UserId { get; set; }
    public bool ReducedAnimations { get; set; } = false;
    public int DefaultSnoozeDurationSeconds { get; set; } = 30 * 60; //20 minutes
    public TimeOnly DailyNotificationTime { get; set; } = new TimeOnly(7, 30); //07:30
    public string IanaTimeZone { get; set; } = "Etc/UTC";
    public List<string> GoalTags { get; set; } = new List<string>();
    public bool DisableEmailNotifications { get; set; } = false;
}
