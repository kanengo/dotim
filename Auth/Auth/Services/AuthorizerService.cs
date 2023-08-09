using Auth.Domain.Models;
using Auth.Infrastructure;
using Auth.Infrastructure.Rpc;
using Grpc.Core;
using Sequence;

namespace Auth.Services;

public class AuthorizerService: Authorizer.AuthorizerBase
{
    private readonly AccountService _accountService;
    private readonly RpcClientAggregator _rpcClientAggregator;
    
    public AuthorizerService(AccountService accountService, RpcClientAggregator rpcClientAggregator)
    {
        _accountService = accountService;
        _rpcClientAggregator = rpcClientAggregator;
    }
    
     public override async Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
    {
        var account = await _accountService.GetByUsernameAsync(request.Account);
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

            await _accountService.CreateAsync(account);
        }

        if (account.Password != request.Password)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "password is not correct"));
        }
        
        
        
        return await base.Login(request, context);
    }
}