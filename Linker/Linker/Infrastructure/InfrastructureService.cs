using Dapr.Client;
using Google.Protobuf;
using Grpc.Net.Client;
using Pb;
using Shared.Topics;


namespace Linker.Infrastructure;

public class InfrastructureService
{
    public DaprClient DaprClient { get; }
    
    public  string PusSubName { get; }

    public IConfiguration Configuration { get; }
    
    public ImLogic.ImLogicClient ImLogicClient { get; }

    public InfrastructureService(IConfiguration configuration)
    {
        Configuration = configuration;
        var daprClientBuilder = new DaprClientBuilder();

        var grpcAddress = configuration["Rpc:GrpcAddress"];
        
        var channel = GrpcChannel.ForAddress(configuration["Rpc:GrpcAddress"] ?? string.Empty);

        ImLogicClient = new ImLogic.ImLogicClient(channel);
        
        if (grpcAddress is not null)
        {
            daprClientBuilder.UseGrpcEndpoint(grpcAddress);
        }

        DaprClient = daprClientBuilder.Build();

        PusSubName = configuration["PubSub:Name"] ?? "pubsub";
    }
    
    public async Task PublishLinkStateEventAsync(IMessage<LinkStateEvent> data, Dictionary<string, string> metadata = default!)
    {   
        
        await _publishByteEventAsync(string.Format(CloudEventTopics.Linker.LinkStateChange, Configuration["Namespace"],Configuration["ServiceName"]), data.ToByteArray(),
            metadata);
    }

    private async Task _publishByteEventAsync(string topicName, ReadOnlyMemory<byte> data, Dictionary<string, string> metadata = default!,
        CancellationToken cancellationToken = default)
    {
        await DaprClient.PublishByteEventAsync(PusSubName, topicName, data,metadata:metadata, cancellationToken:cancellationToken);
    }
}