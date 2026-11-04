using Grpc.Core;

namespace AccountsAPI.Presentation.Helpers;

public static class RpcExceptionHelper
{
    public static RpcException InternalError() => new RpcException(new Status(StatusCode.Internal, $"An internal error occurred. Trace ID: {TraceContextHelper.CurrentTraceId()}"));
}