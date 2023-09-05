using Dapr.AppCallback.Autogen.Grpc.v1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Linker.Domains;
using Linker.Infrastructure;
using Pb;

namespace Linker.Services;

public class DaprAppCallbackService : AppCallback.AppCallbackBase
{
    private readonly ILogger<DaprAppCallbackService> _logger;

    private readonly InfrastructureService _infrastructureService;


    private const string EventTypePushUsers = "nim.linker.push.users";
    
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
                    PubsubName = _infrastructureService.Configuration["PubSub:Name"],
                    Topic = $"{_infrastructureService.Configuration["Namespace"]}/{_infrastructureService.Configuration["ServiceName"]}/{ServiceIdentity.Instance.UniqueId}",
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

    public override async Task<TopicEventResponse> OnTopicEvent(TopicEventRequest request, ServerCallContext context)
    {
        switch (request.Type)
        {
            case EventTypePushUsers:
                var data = OnPushUsers.Parser.ParseFrom(request.Data);
                await ConnectionManager.Instance.SendMessageByUserIds(data.AppId, data.UserIds, data.Data.ToByteArray());
                break;
        }

        return new TopicEventResponse
        {
            Status = TopicEventResponse.Types.TopicEventResponseStatus.Success
        };
    }
    
    
}