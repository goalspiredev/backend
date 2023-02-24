namespace GoalspireBackend.Dto.Response.Auth;

public class LoginResponse
{
    public bool Succeeded { get; set; }
    public string? Token { get; set; }
    public string? Note { get; set; }
}