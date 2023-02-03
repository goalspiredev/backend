namespace GoalspireBackend.Dto.Requests.Notifications;

public class SendNotificationRequest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}