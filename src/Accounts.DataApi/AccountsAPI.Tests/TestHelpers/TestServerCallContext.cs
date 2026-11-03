using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace AccountsAPI.Tests.TestHelpers;

public class TestServerCallContext(string method, CancellationToken ct) : ServerCallContext
{
    protected override CancellationToken CancellationTokenCore => ct;
    protected override string MethodCore => method;

    protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
    {
        throw new NotImplementedException();
    }

    protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions options)
    {
        throw new NotImplementedException();
    }

    protected override string HostCore =>  throw new NotImplementedException();
    protected override string PeerCore =>  throw new NotImplementedException();
    protected override DateTime DeadlineCore =>  throw new NotImplementedException();
    protected override Metadata RequestHeadersCore =>  throw new NotImplementedException();
    protected override Metadata ResponseTrailersCore =>  throw new NotImplementedException();
    protected override Status StatusCore { get; set; }
    protected override WriteOptions WriteOptionsCore { get; set; }
    protected override AuthContext AuthContextCore =>  throw new NotImplementedException();
}