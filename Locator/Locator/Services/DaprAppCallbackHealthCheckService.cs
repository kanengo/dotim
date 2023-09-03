using Dapr.AppCallback.Autogen.Grpc.v1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;


namespace Locator.Services;

public class DaprAppCallbackHealthCheckService : AppCallbackHealthCheck.AppCallbackHealthCheckBase
{
    private readonly ILogger<DaprAppCallbackHealthCheckService> _logger;
    
    
    public DaprAppCallbackHealthCheckService(ILogger<DaprAppCallbackHealthCheckService> logger)
    {
        _logger = logger;
    }

    public override Task<HealthCheckResponse> HealthCheck(Empty request, ServerCallContext context)
    {
        _logger.LogDebug("HealthCheck dapr");
        return Task.FromResult(new HealthCheckResponse());
    }
}