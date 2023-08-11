using System.Runtime.CompilerServices;
using Auth.Domain.Models;
using Auth.Infrastructure.Rpc;
using Auth.Infrastructure;
using Grpc.Core;
using Sequence;

namespace Auth.Services;

public class AuthorizerService: Authorizer.AuthorizerBase
{
    private readonly InfrastructureAggregator _infrastructureAggregator;
    private readonly RpcClientAggregator _rpcClientAggregator;
    
    public AuthorizerService(InfrastructureAggregator infrastructureAggregator, RpcClientAggregator rpcClientAggregator)
    {
        _infrastructureAggregator = infrastructureAggregator;
        _rpcClientAggregator = rpcClientAggregator;
    }
    
     public override async Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
    {
        if (_infrastructureAggregator.AccountService is null)
        {
            throw new RpcException(new Status(StatusCode.Unavailable, "unavailable service"));
        }
        var account = await _infrastructureAggregator.AccountService.GetByUsernameAsync(request.Account);
        if (account is null) //没有则自动创建
        {
            var id = await _rpcClientAggregator.SequencerClient.GetBizIncrementIdAsync(new GetBizIncrementIdRequest
            {
                BizId = "account"
            });
            account = new Account
            {
                Id = id.MaxId,
                CreateAt = DateTime.Now,
                Password = request.Password,
                Status = 1,
                Username = request.Account,
            };

            await _infrastructureAggregator.AccountService.CreateAsync(account);
        }

        if (account.Password != request.Password)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "password is not correct"));
        }

        account.Token = _infrastructureAggregator.Jwt.Create();
        await _infrastructureAggregator.AccountService.UpdateTokenAsync(account.Id, account.Token);
        
        return await Task.FromResult(new LoginReply
        {
            AccountId = account.Id,
            Token = account.Token
        });
    }
}