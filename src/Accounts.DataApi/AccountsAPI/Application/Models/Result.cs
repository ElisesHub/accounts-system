namespace AccountsAPI.Application.Models;

public class Result<T>
{
    public bool IsSuccess { get; set; }
    public bool IsValidationError { get; set; }
    public bool IsNotFound { get; set; }
    public T? Value { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
    public string? Message { get; set; }

    public static Result<T> Success(T value)
    {
        return new Result<T>
        {
            IsSuccess = true,
            Value = value
        };
    }

    public static Result<T> Validation(string field, string message)
    {
        return new Result<T>
        {
            IsValidationError = true,
            Errors = new Dictionary<string, string[]>
            {
                [field] = new[] { message }
            }
        };
    }

    public static Result<T> Validation(Dictionary<string, string[]> errors)
    {
        return new Result<T>
        {
            IsValidationError = true,
            Errors = errors
        };
    }

    public static Result<T> NotFound(string message)
    {
        return new Result<T>
        {
            IsNotFound = true,
            Message = message
        };
    }
}