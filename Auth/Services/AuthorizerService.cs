using Auth.Infrastructure;
using Grpc.Core;

namespace Auth.Services;

public class AuthorizerService: Authorizer.AuthorizerBase
{
    private readonly ILogger<AuthorizerService> _logger;

    private readonly AccountService _accountService;

    public AuthorizerService(ILogger<AuthorizerService> logger,
        AccountService accountService)
    {
        _logger = logger;
        _accountService = accountService;
    }

    public override async Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
    {
        var account = await _accountService.GetByUsernameAsync(request.Account);
        if (account is null)
        {
            var status = new Status(StatusCode.NotFound, "account not exists!");
            throw new RpcException(status);
        }
       
        if (request.Password != account.Password)
        {
            var status = new Status(StatusCode.PermissionDenied, "密码错误");
            throw new RpcException(status);
        }


        return await Task.FromResult(new LoginReply
        {
            AccountId = account.Id,
            Token = account.Token
        });
    }
}