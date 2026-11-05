namespace OrchestrationApi.Application.Common.Errors;

using FluentResults;

public sealed class NotFoundError(string message) : Error(message);