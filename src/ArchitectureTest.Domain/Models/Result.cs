namespace ArchitectureTest.Domain.Models;

// Original implementation taken from here:
// https://medium.com/@wgyxxbf/result-pattern-a01729f42f8c
// Some code of the original implementation was removed
public class Result<TValue, TError>
{
    public readonly TValue? Value;
    public readonly TError? Error;

    // private bool _isSuccess;

    private Result(TValue value)
    {
        Value = value;
        Error = default;
    }

    private Result(TError error)
    {
        Value = default;
        Error = error;
    }

    // Happy path
    public static implicit operator Result<TValue, TError>(TValue value) => new Result<TValue, TError>(value);

    // Error path
    public static implicit operator Result<TValue, TError>(TError error) => new Result<TValue, TError>(error);
}
