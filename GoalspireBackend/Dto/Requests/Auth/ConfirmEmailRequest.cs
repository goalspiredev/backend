using System.ComponentModel.DataAnnotations;

namespace GoalspireBackend.Dto.Requests.Auth;

public class ConfirmEmailRequest
{
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}