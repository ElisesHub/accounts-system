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

                logger.LogError("Unexpected null value found for {ContextTitle} {ContextValue}. Trace ID: {TraceID}.", contextTitle, contextValue, TraceContextHelper.CurrentTraceId());
                throw RpcExceptionHelper.InternalError();


            case ResultStatus.Unknown:
                logger.LogError("Result for {ContextTitle} {ContextValue} had no state set. Trace ID: {TraceID}.", contextTitle, contextValue, TraceContextHelper.CurrentTraceId());
                throw RpcExceptionHelper.InternalError();

            default:
                logger.LogError("Unexpected result {State} for {ContextTitle} {ContextValue}. Trace ID: {TraceID}.",
                    result.CurrentState, contextTitle, contextValue, TraceContextHelper.CurrentTraceId());
                throw RpcExceptionHelper.InternalError();

        }

    }


}