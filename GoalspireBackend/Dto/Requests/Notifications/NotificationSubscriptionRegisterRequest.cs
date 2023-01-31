namespace GoalspireBackend.Dto.Requests.Notifications;

public class NotificationSubscriptionRegisterRequest
{
    public Guid UserId { get; set; }
    public Guid Id { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string p256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
}