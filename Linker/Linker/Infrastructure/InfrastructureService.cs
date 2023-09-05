using Dapr.Client;
using Dapr.Client.Autogen.Grpc.v1;
using Google.Protobuf;
using Grpc.Net.Client;
using NanoidDotNet;
using Pb;
using Shared.Bulk;
using Shared.Topics;


namespace Linker.Infrastructure;

public class InfrastructureService
{
    private readonly DaprClient _daprClient;

    public IConfiguration Configuration { get; }
    
    public ImLogic.ImLogicClient ImLogicClient { get; }

    private readonly Dapr.Client.Autogen.Grpc.v1.Dapr.DaprClient _daprGrpcClient;

    private readonly BulkOperator<BulkPublishRequestEntry> _linkBulkPub;

    private readonly ILogger<InfrastructureService> _logger;

    public InfrastructureService(IConfiguration configuration, ILogger<InfrastructureService> logger)
    {
        _logger = logger;
        
        Configuration = configuration;
        var daprClientBuilder = new DaprClientBuilder();

        var grpcAddress = configuration["Rpc:GrpcAddress"];
        
        var channel = GrpcChannel.ForAddress(configuration["Rpc:GrpcAddress"] ?? string.Empty);

        ImLogicClient = new ImLogic.ImLogicClient(channel);

        _daprGrpcClient =  new Dapr.Client.Autogen.Grpc.v1.Dapr.DaprClient(channel);
        
        if (grpcAddress is not null)
        {
            daprClientBuilder.UseGrpcEndpoint(grpcAddress);
        }

        _daprClient = daprClientBuilder.Build();

        _linkBulkPub = new BulkOperator<BulkPublishRequestEntry>(100, TimeSpan.FromMilliseconds(100));
        _linkBulkPub.BulkHandler += async (_, args) =>
        {
            await _linkPubBulkHandler(args.Data);
        };
    }

    private async Task _linkPubBulkHandler(IReadOnlyCollection<BulkPublishRequestEntry> entries)
    {
        if (entries.Count == 0) return;
        var data = new BulkPublishRequest
        {
            PubsubName = Configuration["Pubsub:Name"],
            Topic = string.Format(CloudEventTopics.Linker.LinkStateChange, Configuration["Namespace"]),
        };
        data.Entries.AddRange(entries);
        
        _logger.LogDebug("_linkPubBulkHandler");
        
        await _daprGrpcClient.BulkPublishEventAlpha1Async(data);
    }
    
    public async Task PublishEventAsync(string topicName, IMessage message, Dictionary<string, string> metadata = default!,
        CancellationToken cancellationToken = default)
    {
        if (topicName == CloudEventTopics.Linker.LinkStateChange)
        {
            var entry = new BulkPublishRequestEntry
            {
                // ReSharper disable once MethodHasAsyncOverload
                EntryId = Nanoid.Generate(),
                Event = message.ToByteString(),
                ContentType = "application/octet-stream"
            };
            
            await _linkBulkPub.WriteAsync(entry);
            return;
        }
        
        
        await _daprClient.PublishByteEventAsync(string.Format(topicName, Configuration["Namespace"]), topicName, message.ToByteArray(),
            metadata:metadata, 
            cancellationToken:cancellationToken);
    }
}