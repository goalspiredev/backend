namespace GoalspireBackend.Models;

public class NotificationSubscription
{
    public Guid UserId { get; set; }
    public Guid Id { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string p256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
}
