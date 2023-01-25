using System.ComponentModel.DataAnnotations;

namespace GoalspireBackend.Dto.Requests.Auth;

public class LoginRequest
{
    //public string Login { get; set; } = string.Empty;
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; } = false;
}