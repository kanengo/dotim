using Dapr.Client;
using Google.Protobuf;
using Grpc.Net.Client;
using Shared.Topics;


namespace Locator.Infrastructure;

public class InfrastructureService
{
    public DaprClient DaprClient { get; }
    
    public  string PusSubName { get; }

    public IConfiguration Configuration { get; }

    public InfrastructureService(IConfiguration configuration)
    {
        Configuration = configuration;
        var daprClientBuilder = new DaprClientBuilder();

        var grpcAddress = configuration["Rpc:GrpcAddress"];
        
        var channel = GrpcChannel.ForAddress(configuration["Rpc:GrpcAddress"] ?? string.Empty);

        if (grpcAddress is not null)
        {
            daprClientBuilder.UseGrpcEndpoint(grpcAddress);
        }

        DaprClient = daprClientBuilder.Build();

        PusSubName = configuration["PubSub:Name"] ?? "pubsub";
    }
    
    
    public async Task SaveState<TValue>(string key, TValue value,  IReadOnlyDictionary<string, string> metadata = default!)
    {
        await DaprClient.SaveStateAsync(Configuration["StateStores:Name"], key, value, metadata:metadata);
    
    }
}