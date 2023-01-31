namespace GoalspireBackend.Dto.Requests.Notifications;

public class SendNotificationRequest
{
    public Guid UserId { get; set; }
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public NotificationType NotificationType { get; set; }
}
public enum NotificationType //TODO: maybe more types
{
    Misc = 0,
    Reminder = 1,
    Goal = 2
}