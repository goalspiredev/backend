namespace GoalspireBackend.Common;

public class Result
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }

    public static Result Success => new Result { Succeeded = true };

    public static Result Failure(string error)
    {
        return new Result
        {
            Succeeded = false,
            Error = error
        };
    }
}