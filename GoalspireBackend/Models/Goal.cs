using GoalspireBackend.Common;

namespace GoalspireBackend.Models;

public enum GoalType
{
    Goal = 0,
    Task = 1
}

public class Goal : AuditableEntity
{
    public Guid UserId { get; set; }
    
    public GoalType Type { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    
    /*
     * urgent - 0 - default
     * important - 1
     * medium - 2
     * small - 3
     */
    public int? Priority { get; set; }

    public DateTime EndsAt { get; set; }
    public bool IsCompleted { get; set; }

    public List<string> Tags { get; set; } = new List<string>();
}