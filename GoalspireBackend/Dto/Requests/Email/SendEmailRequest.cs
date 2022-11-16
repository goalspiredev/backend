namespace GoalspireBackend.Dto.Requests.Email;

public class SendEmailRequest
{
    public string Email { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}