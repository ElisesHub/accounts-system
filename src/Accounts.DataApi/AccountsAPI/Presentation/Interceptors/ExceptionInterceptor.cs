using AccountsAPI.Presentation.Helpers;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace AccountsAPI.Presentation.Interceptors;

/// <summary>
/// Global server interceptor that handles exceptions escaping gRPC service methods.
/// Deliberate <see cref="RpcException"/>s pass through unchanged; cancellations caused by the
/// call (client disconnect or expired deadline) are logged as warnings and rethrown; any other
/// exception is logged once with the trace ID and returned to the caller as a generic
/// <see cref="StatusCode.Internal"/> carrying the trace ID, never the exception details.
/// </summary>
/// <remarks>
/// Covers unary and server-streaming calls, the only call types this API exposes.
/// Override the client-streaming and duplex handlers too if those are added.
/// </remarks>
public class ExceptionInterceptor(ILogger<ExceptionInterceptor> logger) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
        ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        where TRequest : class
        where TResponse : class
    {
        // The shared handler takes a Func<Task>, so the lambda stores the response in this
        // captured local rather than returning it. It is only read if the call completed;
        // every failure path throws before reaching the return.
        TResponse response = null!;
        await CatchExceptions(
            async () =>
            {
                response = await continuation(request, context);
            }, context);

        return response;
    }

    /// <summary>
    /// Wraps a server-streaming call. The continuation's task completes only after the last
    /// message is written, so a failure part-way through the stream is caught here too and
    /// reported in the trailers as an error status, not as a successful, shorter stream.
    /// </summary>
    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request,
        IServerStreamWriter<TResponse> responseStream, ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
        where TRequest : class
        where TResponse : class
    {
        await CatchExceptions(() => continuation(request, responseStream, context), context);
    }

    // Exception policy shared by all call types.
    private async Task CatchExceptions(Func<Task> action, ServerCallContext context)
    {
        try
        {
            await action();
        }
        catch (RpcException)
        {
            // Deliberate statuses (NotFound, InvalidArgument) already carry the right status for the caller.
            // This also covers the translator's own Internal (Unknown / null-value branches),
            // which it has already logged, so nothing is logged twice.
            throw;
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            // The client disconnected or the deadline expired: not a server fault, and nobody is
            // waiting for a reply. Logged as a warning so repeated timeouts are still visible.
            // A cancellation not caused by the call fails this filter and is treated as a fault below.
            logger.LogWarning("Operation Canceled Exception from {Method}. Trace ID: {TraceId}", context.Method, TraceContextHelper.CurrentTraceId());
            throw;
        }
        catch (Exception e)
        {
            // Unexpected fault: log the full exception once, return only a generic message and the
            // trace ID so the caller can report it without internal details being exposed.
            logger.LogError(e, "Unhandled Exception from {Method}. Trace ID: {TraceId}.", context.Method, TraceContextHelper.CurrentTraceId());

            throw RpcExceptionHelper.InternalError();
        }
    }
}