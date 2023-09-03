using Dapr.AppCallback.Autogen.Grpc.v1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

using Shared.Topics;
using static Shared.Topics.CloudEventType;

namespace Locator.Services;

public class DaprAppCallbackService : AppCallback.AppCallbackBase
{
    private readonly ILogger<DaprAppCallbackService> _logger;

    private readonly IConfiguration _configuration;

    public DaprAppCallbackService(ILogger<DaprAppCallbackService> logger,IConfiguration configuration)
    {
        _logger = logger;

        _configuration = configuration;
    }

    public override Task<ListTopicSubscriptionsResponse> ListTopicSubscriptions(Empty request, ServerCallContext context)
    {
        _logger.LogDebug("ListTopicSubscriptions pubsub name:{}, namespage:{}",_configuration["PubSub:Name"],_configuration["Namespace"] );
        var response = new ListTopicSubscriptionsResponse
        {
            Subscriptions =
            {
                new TopicSubscription
                {
                    PubsubName = _configuration["PubSub:Name"],
                    Topic = string.Format(CloudEventTopics.Linker.LinkStateChange, _configuration["Namespace"]),
                    // BulkSubscribe = new BulkSubscribeConfig
                    // {
                    //     Enabled = true,
                    //     MaxMessagesCount = 100,
                    //     MaxAwaitDurationMs = 100,
                    // }
                },
            }
        };
        return Task.FromResult(response);
    }

    public override Task<TopicEventResponse> OnTopicEvent(TopicEventRequest request, ServerCallContext context)
    {
        switch (request.Type)
        {
            case Linker.LinkStateConnect:
                _logger.LogDebug("OnTopicEvent:{}", Linker.LinkStateConnect);
                
                break;
        }

        return Task.FromResult(new TopicEventResponse
        {
            Status = TopicEventResponse.Types.TopicEventResponseStatus.Success
        });
    }
    
    
}