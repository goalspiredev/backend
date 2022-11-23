using System.ComponentModel.DataAnnotations;

namespace GoalspireBackend.Models;

public enum GoalVisibility
{
    Private = 0, Public = 1
}

public class Settings
{
    [Key]
    public Guid UserId { get; set; }
    public bool ReducedAnimations { get; set; }// = false;
    public TimeSpan DefaultSnoozeDuration { get; set; }// = TimeSpan.FromMinutes(30);
    public TimeOnly DailyNotificationTime { get; set; }// = new TimeOnly(7, 30); //07:30
    public GoalVisibility DefaultGoalVisibility { get; set; }// = GoalVisibility.Private; // nebo tohle muze byt "bool DefaultGoalsArePublic = false"
    public List<string> GoalTags { get; set; } = new List<string>();
    public bool DisableEmailNotifications { get; set; }//= false
}
