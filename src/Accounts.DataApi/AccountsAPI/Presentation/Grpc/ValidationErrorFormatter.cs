namespace AccountsAPI.Presentation.Grpc;

public static class ValidationErrorFormatter
{
    public static string Format(IReadOnlyDictionary<string, string[]>? errors)
    {
        if (errors is null || errors.Count == 0)
        {
            return "Invalid request.";
        }

        string fieldErrors = string.Join("; ", errors.Select(error =>
            $"{error.Key}: {string.Join(", ", error.Value)}"));

        return $"Invalid data: {fieldErrors}";
    }
}