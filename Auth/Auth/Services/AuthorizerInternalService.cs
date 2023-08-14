using Auth.Infrastructure;
using Grpc.Core;
using static Grpc.Core.StatusCode;

namespace Auth.Services;

public class AuthorizerInternalService: AuthorizerInternal.AuthorizerInternalBase
{
    private readonly InfrastructureAggregator _infrastructureAggregator;

    public AuthorizerInternalService(InfrastructureAggregator infrastructureAggregator)
    {
        _infrastructureAggregator = infrastructureAggregator;
    }
    
    public override async Task<CheckTokenValidReply> CheckTokenValid(CheckTokenValidRequest request, ServerCallContext context)
    {
        var account =  await _infrastructureAggregator.AccountService?.GetByTokenAsync(request.Token)!;
        if (account is null)
            throw new RpcException(new Status(statusCode: Unauthenticated, "token is invalid"));

        return await Task.FromResult(new CheckTokenValidReply
        {
            Id = account.Id
        });
    }
}