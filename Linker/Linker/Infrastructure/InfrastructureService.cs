using Dapr.Client;
using Google.Protobuf;
using Grpc.Net.Client;
using Pb;


namespace Linker.Infrastructure;

public class InfrastructureService
{
    public DaprClient DaprClient { get; }
    
        
    private readonly string _pusSubName;

    private readonly IConfiguration _configuration;
    
    public ImLogic.ImLogicClient ImLogicClient { get; }

    public InfrastructureService(IConfiguration configuration)
    {
        _configuration = configuration;
        var daprClientBuilder = new DaprClientBuilder();

        var grpcAddress = configuration["Rpc:GrpcAddress"];
        
        var channel = GrpcChannel.ForAddress(configuration["Rpc:GrpcAddress"] ?? string.Empty);

        ImLogicClient = new ImLogic.ImLogicClient(channel);
        
        if (grpcAddress is not null)
        {
            daprClientBuilder.UseGrpcEndpoint(grpcAddress);
        }

        DaprClient = daprClientBuilder.Build();

        _pusSubName = configuration["PubSub:Name"] ?? "pulsar";
    }
    
    public async Task PublishLinkStateEventAsync(IMessage<LinkStateEvent> data, Dictionary<string, string> metadata = default!)
    {
        await PublishByteEventAsync($"{_configuration["AppName"]}/{_configuration["ServiceName"]}", data.ToByteArray(),
            metadata);
    }

    public async Task PublishByteEventAsync(string topicName, ReadOnlyMemory<byte> data, Dictionary<string, string> metadata = default!,
        CancellationToken cancellationToken = default)
    {
        await DaprClient.PublishByteEventAsync(_pusSubName, topicName, data, "application/grpc",
            metadata, cancellationToken);
    }
}