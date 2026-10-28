namespace AccountsAPI.Presentation.Grpc;

/// <summary>
/// Formats application validation errors into a single string for a gRPC
/// <see cref="Grpc.Core.Status"/> detail, which can only carry text.
/// </summary>
/// <remarks>
/// Temporary until the gRPC rich error model is adopted, which sends field errors as structured data.
/// </remarks>
public static class ValidationErrorFormatter
{
    /// <summary>
    /// Flattens field errors into one readable message,
    /// e.g. <c>"Invalid data: accountId: Must be positive; firstName: Required"</c>.
    /// </summary>
    /// <param name="errors">Field names mapped to their error messages. May be null or empty.</param>
    /// <returns>
    /// The formatted message, or a generic <c>"Invalid request."</c> if there are no field errors.
    /// </returns>
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