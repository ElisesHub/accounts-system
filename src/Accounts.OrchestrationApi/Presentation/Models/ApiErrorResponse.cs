namespace PortfolioApplicationAPI.Presentation.Models;

public class ApiErrorResponse
{
    public string Code { get; set; } = default!;
    public string Message { get; set; } = default!;
    public Dictionary<string, string[]>? FieldErrors { get; set; }
    public string? TraceId { get; set; }

    public string? DeveloperMessage { get; set; }
}