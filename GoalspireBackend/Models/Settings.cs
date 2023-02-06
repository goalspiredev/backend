using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoalspireBackend.Models;

public class Settings
{
    [Key]
    public Guid UserId { get; set; }
    public bool ReducedAnimations { get; set; } = false;
    public TimeSpan DefaultSnoozeDuration { get; set; } = TimeSpan.FromMinutes(30);
    public TimeOnly DailyNotificationTime { get; set; } = new TimeOnly(7, 30); //07:30

    [NotMapped]
    public TimeZoneInfo TimeZone
    {
        get
        {
            return TimeZoneInfo.FindSystemTimeZoneById(IanaTimeZone);
        }
        set
        {
            TimeZoneInfo.TryConvertWindowsIdToIanaId(value.Id, out string ianaId);
            ianaId ??= "Etc/UTC";
            IanaTimeZone = ianaId;
        }
    }
    public string IanaTimeZone { get; set; } = "Etc/UTC";

    public List<string> GoalTags { get; set; } = new List<string>();
    public bool DisableEmailNotifications { get; set; } = false;
}
