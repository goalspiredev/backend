using GoalspireBackend.Common;

namespace GoalspireBackend.Models;

public enum GoalType
{
    Goal = 0,
    Task = 1
}

public class Goal : AuditableEntity
{
    public Guid UserId { get; init; }
    
    public GoalType Type { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    
    public int? Priority { get; set; }
    public DateTime EndsAt { get; set; }
    
    public bool IsCompleted { get; set; }
    public bool IsPublic { get; set; }
}