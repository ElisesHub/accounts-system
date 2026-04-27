using FluentResults;
namespace PortfolioApplicationAPI.Application.Common.Errors;

public class ValidationError(string message) : Error(message);