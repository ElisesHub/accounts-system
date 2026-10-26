namespace AccountsAPI.Application.Models;

public class Result<T>
{
    public ResultStatus CurrentState { get; private set; }
    public T? Value { get; private set; }
    public IReadOnlyDictionary<string, string[]>? Errors { get; private set; }
    public string? Message { get; private set; }

    private Result(){}


    public static Result<T> Success(T value)
    {
        return new Result<T>
        {
            CurrentState = ResultStatus.IsSuccess,
            Value = value
        };
    }

    public static Result<T> Validation(string field, string message)
    {
        return new Result<T>
        {
            CurrentState = ResultStatus.IsValidationError,
            Errors = new Dictionary<string, string[]>
            {
                [field] = [message]
            }
        };
    }

    public static Result<T> Validation(IReadOnlyDictionary<string, string[]> errors)
    {
        return new Result<T>
        {
            CurrentState = ResultStatus.IsValidationError,
            Errors = errors
        };
    }

    public static Result<T> NotFound(string message)
    {
        return new Result<T>
        {
            CurrentState = ResultStatus.IsNotFound,
            Message = message
        };
    }
}

public enum ResultStatus
{
    Unknown = 0, IsSuccess, IsValidationError, IsNotFound
}