using System.ComponentModel.DataAnnotations;

namespace GoalspireBackend.Dto.Requests.Auth;

public class ForgotPasswordRequest
{
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
