using Grpc.Net.Client;
using Pb;

namespace Comet.Infrastructure;

public class InfrastructureService
{
    public ImLogic.ImLogicClient ImLogic { get; }

    public InfrastructureService(IConfiguration configuration)
    {   
        var channel = GrpcChannel.ForAddress(configuration["Rpc:GrpcAddress"] ?? string.Empty);

        ImLogic = new ImLogic.ImLogicClient(channel);
    }
}