namespace GoalspireBackend.Dto.Requests.Email;

public class SendEmailRequest
{
    public string Email { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = false;
}

public class SendEmailVerificationEmailRequest
{
    public string Email { get; set; } = string.Empty;
    public string Title { get; set; } = "Email confirmation";
    public string UserName { get; set; } = string.Empty;
    public string ConfirmURL { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;

}