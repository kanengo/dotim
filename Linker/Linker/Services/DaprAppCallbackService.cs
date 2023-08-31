using Dapr.AppCallback.Autogen.Grpc.v1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Linker.Domains;
using Linker.Infrastructure;

namespace Linker.Services;

public class DaprAppCallbackService : AppCallback.AppCallbackBase
{
    private readonly ILogger<DaprAppCallbackService> _logger;

    private readonly InfrastructureService _infrastructureService;
    
    public DaprAppCallbackService(ILogger<DaprAppCallbackService> logger,InfrastructureService infrastructureService)
    {
        _logger = logger;

        _infrastructureService = infrastructureService;
    }

    public override Task<ListTopicSubscriptionsResponse> ListTopicSubscriptions(Empty request, ServerCallContext context)
    {
        var response = new ListTopicSubscriptionsResponse
        {
            Subscriptions =
            {
                new TopicSubscription
                {
                    PubsubName = _infrastructureService.PusSubName,
                    Topic = $"{_infrastructureService.Configuration["AppName"]}/{_infrastructureService.Configuration["ServiceName"]}/{ServiceIdentity.Instance.UniqueId}",
                    Routes = null,
                    DeadLetterTopic = null,
                    BulkSubscribe = new BulkSubscribeConfig
                    {
                        Enabled = true,
                        MaxMessagesCount = 1000,
                        MaxAwaitDurationMs = 1000
                    }
                },
            }
        };
        return Task.FromResult(response);
    }

    public override Task<TopicEventResponse> OnTopicEvent(TopicEventRequest request, ServerCallContext context)
    {
        return base.OnTopicEvent(request, context);
    }
    
    
}