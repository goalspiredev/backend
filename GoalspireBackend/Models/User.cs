namespace GoalspireBackend.Models;

public class User
{
    public Guid Id { get; set; }
    
    public string Email { get; set; }
    public bool EmailConfirmed { get; set; }
    
    public string PasswordHash { get; set; }
    public string Name { get; set; }
}