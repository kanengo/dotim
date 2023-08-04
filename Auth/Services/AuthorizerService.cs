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
            
        }
        else
        {
            if (request.Password != account.Password)
            {
                var status = new Status(StatusCode.PermissionDenied, "密码错误");
                throw new RpcException(status);
            }
        }


        return Task.FromResult(new LoginReply
        {
            Token = "",
            AccountId = 0
        });
    }
}