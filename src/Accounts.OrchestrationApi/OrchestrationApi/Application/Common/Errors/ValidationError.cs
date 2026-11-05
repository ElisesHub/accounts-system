using FluentResults;
namespace OrchestrationApi.Application.Common.Errors;

public class ValidationError(string message) : Error(message);