
using Grpc.Core;
using Pb;

namespace ImLogic.Services;

public class ImLogicService : Pb.ImLogic.ImLogicBase
{
    private readonly ILogger<ImLogicService> _logger;

    public ImLogicService(ILogger<ImLogicService> logger)
    {
        _logger = logger;
    }

    public override Task<AuthenticateReply> Authenticate(AuthenticateRequest request, ServerCallContext context)
    {   
        return Task.FromResult(new AuthenticateReply
        {
            AppId = "1001",
            UserId = "6544",
            DeviceType = DeviceType.Android,
        });
    }
}