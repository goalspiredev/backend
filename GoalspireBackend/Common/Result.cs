namespace GoalspireBackend.Common;

public class Result
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }

    public static Result Success => new Result { Succeeded = true };
}