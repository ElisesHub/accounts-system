using System.Diagnostics;
using AccountsAPI.Application.Models;
using AccountsAPI.Presentation.Grpc;
using Grpc.Core;

namespace AccountsAPI.Presentation.Helpers;

public static class ResultTranslationHelper
{
    public static T GetValueOrThrow<T>(this Result<T> result, ILogger logger, string contextTitle, string contextValue)
    {
        switch (result.CurrentState)
        {
            case ResultStatus.NotFound:
                throw new RpcException(new Status(StatusCode.NotFound, result.Message ?? $"{contextTitle} not found."));

            case ResultStatus.ValidationError:
                throw new RpcException(new Status(StatusCode.InvalidArgument, ValidationErrorFormatter.Format(result.Errors) ?? "An error occurred during validation"));

            case ResultStatus.Success:
                if (result.Value is not null) return result.Value;

                logger.LogError("Unexpected null value found for {ContextTitle} {ContextValue}", contextTitle, contextValue);
                throw InternalError();


            case ResultStatus.Unknown:
                logger.LogError("Result for {ContextTitle} {ContextValue} had no state set", contextTitle, contextValue);
                throw InternalError();
            default:
                logger.LogError("Unexpected result {State} for {ContextTitle} {ContextValue}",
                    result.CurrentState, contextTitle, contextValue);
                throw InternalError();

        }

    }

    private static RpcException InternalError() => new RpcException(new Status(StatusCode.Internal, $"An internal error occurred. Trace ID: {Activity.Current?.TraceId.ToString() ?? "unavailable"}"));
}