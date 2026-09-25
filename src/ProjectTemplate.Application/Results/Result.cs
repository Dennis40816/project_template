namespace ProjectTemplate.Application.Results;

/// <summary>The outcome of a use case: a value, or one or more issues.</summary>
public sealed class Result<T>
{
    internal Result(T? value, IReadOnlyList<Issue> issues)
    {
        Value = value;
        Issues = issues;
    }

    public T? Value { get; }

    public IReadOnlyList<Issue> Issues { get; }

    public bool IsSuccess => Issues.Count == 0;
}

/// <summary>Factory methods for <see cref="Result{T}"/>.</summary>
public static class Result
{
    public static Result<T> Success<T>(T value) => new(value, []);

    public static Result<T> Failure<T>(Issue issue)
    {
        ArgumentNullException.ThrowIfNull(issue);
        return new Result<T>(default, [issue]);
    }
}
