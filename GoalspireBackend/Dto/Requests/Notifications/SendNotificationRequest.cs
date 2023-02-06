namespace GoalspireBackend.Dto.Requests.Notifications;

public class SendNotificationRequest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public NotificationType NotificationType { get; set; }
}

public enum NotificationType
{
    Misc = 0,
    Reminder = 1,
    Goal = 2
}